namespace OnefinityProbeAutoCalibration.Services
{
    public interface IUserService
    {
        decimal RequestDistanceFromUser(decimal preSetDistance, string descriptiveText);
    }
}