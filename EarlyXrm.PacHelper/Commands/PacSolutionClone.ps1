param(
	$ItemPath # solution file path
)

write-host "solution file path: $ItemPath"

if (Test-Path -Path $ItemPath -PathType Container) {
	write-warning "A valid solution file is required!"
	exit 1
}

$directory = [System.IO.Path]::GetDirectoryName($ItemPath)

$solutionName = read-host -Prompt "Solution unique name"

cd $directory

#Clone solution using pac tool
pac solution clone -n $solutionName

Remove-Item .\$solutionName\.gitignore

# uncomment "Solution Package Type" settings and set to both
$cdsprojPath = "$solutionName\$solutionName.cdsproj"

$cdsprojContent = Get-Content $cdsprojPath -Raw

$settings = "<!--\s+<PropertyGroup>\s+<SolutionPackageType>Managed</SolutionPackageType>\s+<SolutionPackageEnableLocalization>false</SolutionPackageEnableLocalization>\s+</PropertyGroup>\s+-->"

$newSettings = @"
<PropertyGroup>
    <SolutionPackageType>Both</SolutionPackageType>
    <SolutionPackageEnableLocalization>false</SolutionPackageEnableLocalization>
  </PropertyGroup>
"@

$cdsprojContent = $cdsprojContent -replace $settings, $newSettings

Set-Content -Value $cdsprojContent -Path $cdsprojPath

# Include cdsproj file in solution
$content = [xml](gc $ItemPath)

$configs = [xml]"<ItemGroup>
  <None Include=`"$solutionName\$solutionName.cdsproj`" />
</ItemGroup>"

$project = $content.Project

$importNode = $content.ImportNode($configs.DocumentElement, $true)

$project.AppendChild($importNode)

# Add cdsproj post build command
$postBuild = "<Exec Command=`"dotnet build $solutionName.cdsproj -p:ProjectDir='`$(ProjectDir)'`" WorkingDirectory=`"`$(ProjectDir)/$solutionName`" />"

$postBuildNode = $project.Target | ? { $_.AfterTargets -eq "PostBuildEvent" }
if ($null -ne $postBuildNode){
	$target = [xml]$postBuild
	$importTarget = $content.ImportNode($target.DocumentElement, $true)
	$postBuildNode.AppendChild($importTarget)
} else {
	$target = [xml]"<Target Name=`"PostBuild`" AfterTargets=`"PostBuildEvent`">$postBuild</Target>"
	$importTarget = $content.ImportNode($target.DocumentElement, $true)
	$project.AppendChild($importTarget)
}

$content.Save($ItemPath)