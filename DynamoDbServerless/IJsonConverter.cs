namespace DynamoDbServerless
{
    public interface IJsonConverter
    {
        string SerializeObject(object obj);
        T DeserializeObject<T>(string content);
    }
}