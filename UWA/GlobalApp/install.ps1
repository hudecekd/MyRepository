$ip = "192.168.0.100"
[array] $directories = (Get-ChildItem -Path "D:\data\MyRepository\UWA\GlobalApp\GlobalApp\AppPackages" | Where-Object {$_.mode -match "d"} | Select-Object Name)

[string] $bestName = ""
[int] $bestMajorNumber = 0
[int] $bestMinorNumber = 0
[int] $bestBuildNumber = 0
[int] $bestRevisionNumber = 0

for ($i = 0; $i -lt $directories.Length; $i++)
{
    $name = $directories[$i].Name
    $splittedName = $name.Split(".", [System.StringSplitOptions]::RemoveEmptyEntries)

    $majorSplit = $splittedName[0].ToString()
    $major = $majorSplit.Split("_", [System.StringSplitOptions]::RemoveEmptyEntries)[1]
    $minor = $splittedName[1]
    $build = $splittedName[2]
    $revision = $splittedName[3].Split("_", [System.StringSplitOptions]::RemoveEmptyEntries)[0]

    #$major + "." + $minor + "." + $build + "." + $revision

    [int] $majorNumber = [convert]::ToInt32($major)
    [int] $minorNumber = [convert]::ToInt32($minor)
    [int] $buildNumber = [convert]::ToInt32($build)
    [int] $revisionNumber = [convert]::ToInt32($revision)

    if (($bestMajorNumber -lt $majorNumber) -or
       (($bestMajorNumber -eq $majorNumber) -and ($bestMinorNumber -lt $minorNumber)) -or
       (($bestMajorNumber -eq $majorNumber) -and ($bestMinorNumber -eq $minorNumber) -and ($bestBuildNumber -lt $buildNumber)) -or
       (($bestMajorNumber -eq $majorNumber) -and ($bestMinorNumber -eq $minorNumber) -and ($bestBuildNumber -eq $buildNumber) -and ($bestRevisionNumber -lt $revisionNumber)))
    {
        $bestMajorNumber = $majorNumber
        $bestMinorNumber = $minorNumber
        $bestBuildNumber = $buildNumber
        $bestRevisionNumber = $revisionNumber
        $bestName = $name
    }
}

$packageFile = "d:\data\MyRepository\UWA\GlobalApp\GlobalApp\AppPackages\GlobalApp_{0}.{1}.{2}.{3}_Debug_Test\GlobalApp_{0}.{1}.{2}.{3}_arm_Debug.appxbundle" -f $bestMajorNumber, $bestMinorNumber, $bestBuildNumber, $bestRevisionNumber

Write-Output "`r`n"
Write-Output "`r`n"
Write-Output "-----------------------------------------------"
Write-Output ("Newest package version: {0}.{1}.{2}.{3}" -f $bestMajorNumber, $bestMinorNumber, $bestBuildNumber, $bestRevisionNumber)
Write-Output ("Package file to deploy: {0}" -f $packageFile)
Write-Output "`r`n"
Write-Output "`r`n"

& 'C:\Program Files (x86)\Windows Kits\10\bin\x86\WinAppDeployCmd.exe' install -ip $ip -file $packageFile