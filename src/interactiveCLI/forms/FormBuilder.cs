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
    
    public List<Action<T>> GenerateInputActions(Prompt prompt, List<PropertyInfo> properties)
    {
        var actions = new List<Action<T>>();
        
        

        foreach (var property in properties)
        {
            var inputAttribute = property.GetCustomAttribute<InputAttribute>();
            if (inputAttribute == null)
            {
                continue;
            }
            if (inputAttribute is PasswordAttribute)
            {
                actions.Add(instance =>
                {
                    var value = prompt.AskPassword(inputAttribute.Label);
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
        
        var actions = GenerateInputActions(prompt, inputs);
        
        Form<T> form = new Form<T>(instance, actions);
        
        return form;
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