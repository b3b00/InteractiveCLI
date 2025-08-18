using System.Reflection;

namespace interactiveCLI.forms;

public class FormBuilder<T>
{
    public List<PropertyInfo> GetInputFields()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute(typeof(InputAttribute)) != null)
            .ToList();
    }
    
    public List<MethodInfo> GetMethods()
    {
        return typeof(T)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute(typeof(InputConverterAttribute)) != null ||
                        f.GetCustomAttributes(typeof(InputValidatorAttribute)) != null).ToList();

    }
    
    public List<Action<T>> GenerateInputActions(Prompt prompt, List<PropertyInfo> properties, List<MethodInfo> methods)
    {
        var actions = new List<Action<T>>();
        
        Dictionary<string, MethodInfo> validators = new Dictionary<string, MethodInfo>();
        Dictionary<string, MethodInfo> inputConverters = new Dictionary<string, MethodInfo>();

        foreach (var method in methods)
        {
            var validator = method.GetCustomAttributes<InputValidatorAttribute>().FirstOrDefault();
            var parameters = method.GetParameters();
            if (method.ReturnType == typeof(bool) && parameters.Length == 1 &&
                parameters[0].ParameterType == typeof(string))
            {
                if (validator != null && !string.IsNullOrEmpty(validator.InputName))
                {
                    validators[validator.InputName] = method;
                }
            }
            if (parameters.Length == 1 &&
                parameters[0].ParameterType == typeof(string))
            {
                var converter = method.GetCustomAttribute<InputConverterAttribute>();
                if (converter != null && !string.IsNullOrEmpty(converter.InputName))
                {
                    inputConverters[converter.InputName] = method;
                }
            }
        }
        

        foreach (var property in properties)
        {   
            var inputAttribute = property.GetCustomAttribute<InputAttribute>();
            if (inputAttribute == null)
            {
                continue;
            }
            
            else if (inputAttribute is SelectAttribute selectAttribute)
            {
                actions.Add(instance =>
                {
                    var value = prompt.Select(selectAttribute.Label,choices:selectAttribute.Values);
                    property.SetValue(instance, value);
                });
            }
            else if (inputAttribute is PasswordAttribute)
            {
                actions.Add(instance =>
                {
                    var value = prompt.AskPassword(inputAttribute.Label);
                    property.SetValue(instance, value);
                });
            }
            else if (inputAttribute is InputAttribute genericInputAttribute)
            {
                actions.Add(instance =>
                {
                    Predicate<string> validator = null;
                    if (validators.TryGetValue(property.Name, out var validatorMethod))
                    {
                        validator = (string i) =>
                        {
                            validatorMethod.Invoke(instance, new object[] { i });
                            return true;
                        };    
                    }

                    var converterType = typeof(Func<,>).MakeGenericType(typeof(string), property.PropertyType);
                    object converter = null;
                    if (inputConverters.TryGetValue(property.Name, out var converterMethod))
                    {
                        if (converterMethod != null && converterMethod.ReturnType == property.PropertyType)
                        {
                            converter = (string i) =>
                            {
                                //var genericMethod = converterMethod.MakeGenericMethod(property.PropertyType);
                                return Convert.ChangeType(converterMethod.Invoke(instance, new object[] { i }), property.PropertyType);
                            };
                        }
                    }

                    var methods = prompt.GetType().GetMethods().ToList();
                    
                    var askMethod = prompt.GetType().GetMethod("Ask");

                    var realConverter = converter != null ? Convert.ChangeType(converter, converterType) : null;
                    var parameters = new Dictionary<string, object>()
                    {
                        { "label", inputAttribute.Label },
                        { "validator", validator },
                        { "converter", converter },
                        { "possibleValues", inputAttribute.PossibleValues }
                    };
                    var genMeth = askMethod.MakeGenericMethod(property.PropertyType);
                    var value = InvokeWithNamedParameters(genMeth,prompt,parameters);
                    
                    property.SetValue(instance, value);
                });
            }
            else if (property.PropertyType == typeof(int))
            {
                actions.Add(instance =>
                {
                    var value = prompt.AskInt(inputAttribute.Label);
                    property.SetValue(instance, value);
                });
            }
            else if (property.PropertyType == typeof(double))
            {
                actions.Add(instance =>
                {
                    var value = prompt.AskDouble(inputAttribute.Label);
                    property.SetValue(instance, value);
                });
            }
            else if (property.PropertyType == typeof(string))
            {
                actions.Add((instance) =>
                {
                    var value = prompt.AskText(inputAttribute.Label, pattern:inputAttribute.Pattern);
                    property.SetValue(instance, value);
                });
            }
            else if (property.PropertyType == typeof(bool))
            {
                
                actions.Add((instance) =>
                {
                    var boolAttribute = inputAttribute as  BoolInputAttribute;
                    var value = prompt.AskBool(boolAttribute.Label,boolAttribute.TrueValues,boolAttribute.FalseValues);
                    property.SetValue(instance, value);
                });
            }
            
        }

        return actions;
    }

    public Form<T> Build(T instance, Prompt prompt)
    {
        var inputs = GetInputFields();
        var methods = GetMethods();
        
        var actions = GenerateInputActions(prompt, inputs, methods);
        
        Form<T> form = new Form<T>(instance, actions);
        
        return form;
    }
    
    
// method : MethodInfo de la méthode à appeler
// target : instance cible (null si statique)
// namedArgs : dictionnaire nom du paramètre => valeur
    private static object InvokeWithNamedParameters(MethodInfo method, object target, Dictionary<string, object> namedArgs)
    {
        var parameters = method.GetParameters();
        var args = parameters.Select(p => namedArgs.ContainsKey(p.Name) ? namedArgs[p.Name] : Type.Missing).ToArray();
        return method.Invoke(target, args);
    }
}

public class Form<T>
{
    public T Instance { get; set; }
    
    public List<Action<T>> Inputs { get; set; }

    public Form(T instance, List<Action<T>> inputs)
    {
        Instance = instance;
        Inputs = inputs;
    }

    public T Ask()
    {
        foreach (var input in Inputs)
        {
            input(Instance);
        }
        return Instance;
    } 
}