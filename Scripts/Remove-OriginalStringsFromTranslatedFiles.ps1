# Fix transifex files (percentage complete / untranslated strings)
# Remove all untranslated/original strings from the translated files.

[xml]$orgFile = Get-Content D:\GitHub_Repositories\NETworkManager\Source\NETworkManager.Localization\Resources\Strings.resx -Encoding UTF8

foreach($file in Get-ChildItem "D:\GitHub_Repositories\NETworkManager\Source\NETworkManager.Localization\Resources\Strings.*.resx")
{
    [xml]$diffFile = Get-Content $file.FullName -Encoding UTF8

    foreach($data in $orgFile.root.data)
    {
        foreach($data2 in $diffFile.root.data)
        {
            if($data.name -eq $data2.name -and $data.value -eq $data2.value)
            {
                [void]$diffFile.root.RemoveChild($data2)
            }
        }
    }

    $diffFile.Save($file.FullName)
}