using System.Globalization;
using System.Text;

namespace Brendigo.Web2APK.Core;

/// <summary>
/// Creates a stable, valid Android application ID for each new project.
/// The GUID suffix prevents apps with the same displayed name from colliding.
/// A saved project's package ID is never recalculated when its name changes.
/// </summary>
public static class AndroidPackageIdGenerator
{
    public static string Suggest(string? name,Guid projectId)
    {
        if(projectId==Guid.Empty)
            throw new ArgumentException("Projekt mora imati jedinstveni identifikator.",nameof(projectId));
        var normalized=(name ?? "").Normalize(NormalizationForm.FormD);
        var text=new StringBuilder(58);
        bool separator=false;
        foreach(var character in normalized)
        {
            if(CharUnicodeInfo.GetUnicodeCategory(character)==UnicodeCategory.NonSpacingMark)
                continue;
            var lower=char.ToLowerInvariant(character);
            if(lower=='đ')lower='d';
            if(lower is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                if(separator && text.Length>0 && text.Length<54)text.Append('_');
                separator=false;
                if(text.Length>=54)break;
                text.Append(lower);
            }
            else if(text.Length>0)separator=true;
        }
        var slug=text.ToString().Trim('_');
        if(slug.Length==0)slug="app";
        if(slug[0] is < 'a' or > 'z')slug="app_"+slug;
        return "com.brendigo."+slug+"_"+projectId.ToString("N")[..12];
    }

    public static ProjectDefinition NewProject()
    {
        var project=new ProjectDefinition();
        project.PackageName=Suggest(project.Name,project.Id);
        return project;
    }
}
