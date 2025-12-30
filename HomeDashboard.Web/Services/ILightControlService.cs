namespace HomeDashboard.Web.Services;

public interface ILightControlService
{
    Task StartTurnOnAsync(ushort targetBrightness);
    Task StartTurnOffAsync();
}