using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class LocalLicenseRemovalServiceTests
{
    [Fact]
    public void RemoveLocalLicense_ShouldDeleteSignedAndLegacyLicenseOnly()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "xdtbox-license-remove-" + Guid.NewGuid().ToString("N"));
        var paths = new AppDataPathProvider().GetPaths(baseFolder);
        var service = new LocalLicenseRemovalService();

        try
        {
            Directory.CreateDirectory(paths.LicensesFolder);
            Directory.CreateDirectory(paths.LicenseRequestsFolder);
            Directory.CreateDirectory(paths.ProfilesFolder);

            var signedLicenseFile = LocalLicenseRemovalService.GetSignedLicenseFilePath(paths);
            var customerDataFile = Path.Combine(paths.LicensesFolder, "license-customer-data.json");
            var requestFile = Path.Combine(paths.LicenseRequestsFolder, "request.json");
            var profileFile = Path.Combine(paths.ProfilesFolder, "profile.json");

            File.WriteAllText(signedLicenseFile, "signed");
            File.WriteAllText(paths.LicenseFile, "legacy");
            File.WriteAllText(customerDataFile, "customer");
            File.WriteAllText(requestFile, "request");
            File.WriteAllText(profileFile, "profile");

            var result = service.RemoveLocalLicense(paths);

            Assert.True(result.RemovedSignedLicense);
            Assert.True(result.RemovedLegacyLicense);
            Assert.False(File.Exists(signedLicenseFile));
            Assert.False(File.Exists(paths.LicenseFile));
            Assert.True(File.Exists(customerDataFile));
            Assert.True(File.Exists(requestFile));
            Assert.True(File.Exists(profileFile));
        }
        finally
        {
            if (Directory.Exists(baseFolder))
            {
                Directory.Delete(baseFolder, recursive: true);
            }
        }
    }

    [Fact]
    public void RemoveLocalLicense_ShouldReportNoRemovalWhenNoLocalLicenseExists()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "xdtbox-license-remove-" + Guid.NewGuid().ToString("N"));
        var paths = new AppDataPathProvider().GetPaths(baseFolder);
        var service = new LocalLicenseRemovalService();

        try
        {
            var result = service.RemoveLocalLicense(paths);

            Assert.False(result.RemovedAnyLicense);
        }
        finally
        {
            if (Directory.Exists(baseFolder))
            {
                Directory.Delete(baseFolder, recursive: true);
            }
        }
    }
}
