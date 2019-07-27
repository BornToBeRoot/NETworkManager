# Rename language files from transifex

$items = Get-ChildItem -Path $PSScriptRoot\language_files

foreach($item in $items)
{
    if($item -like "*en_US*")
    {
        Remove-Item -Path $item.FullName -Confirm:$false
    }
    elseif($item.Name -like "*Strings_*")
    {
        $newName = $item.Name.Replace("Strings_", "Strings.").Replace("_","-")

        Rename-Item -Path $item.FullName -NewName $newName
    }
}