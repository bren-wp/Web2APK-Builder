namespace Brendigo.Web2APK.Core;

/// <summary>
/// Windows Portable and Setup editions are permanently activation-free.
/// The independent Android mobile client retains its existing BL2 license model.
/// </summary>
public static class WindowsEdition
{
    public const bool LicenseRequired = false;

    // Synthetic internal capability descriptor. This is never issued or
    // persisted as a signed BL2 customer license.
    internal static BrendigoLicense.Claims Capabilities { get; } = new(
        "00000000000000000000000000000000",
        "windows",0,0,LicenseEntitlements.Developer,"*");
}
