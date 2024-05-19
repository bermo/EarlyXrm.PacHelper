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

pac solution clone -n $solutionName

Remove-Item .\$solutionName\.gitignore

$content = [xml](gc $ItemPath);

$configs = [xml]"<ItemGroup>
  <None Include=`"$solutionName\$solutionName.cdsproj`" />
</ItemGroup>
"

$importNode = $content.ImportNode($configs.DocumentElement, $true)

$target = [xml]"<Target Name=`"PostBuild`" AfterTargets=`"PostBuildEvent`">
  <Exec Command=`"dotnet build $solutionName.cdsproj -p:ProjectDir='`$(ProjectDir)'`" WorkingDirectory=`"`$(ProjectDir)/$solutionName`" />
</Target>
"

$importTarget = $content.ImportNode($target.DocumentElement, $true)

$project = $content.Project

$project.AppendChild($importNode)
$project.AppendChild($importTarget)

$content.Save($ItemPath)