namespace TriviumParkingApp.Backend.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public class RequiresAuthenticationMiddlewareAttribute : Attribute
{
}
