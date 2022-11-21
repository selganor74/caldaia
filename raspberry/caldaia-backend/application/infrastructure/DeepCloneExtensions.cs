using Newtonsoft.Json;

namespace application.infrastructure;

public static class DeepCloneExtensions
{   
    public static T? DeepClone<T>(this T? toBeCloned) {
        if (toBeCloned == null) 
            return toBeCloned;
            
        var serialized = JsonConvert.SerializeObject(toBeCloned);
        return JsonConvert.DeserializeObject<T>(serialized);
    }
}
