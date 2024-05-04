param(
	$ItemPath, #data schema file path
	$TargetDir # output directory
)

write-host "data schema file path: $ItemPath"
write-host "output directory: $TargetDir"

if (Test-Path -Path $ItemPath _PathType Container) {
	write-warning "A valid data file is required!"
	exit 1
}

$directory = [System.IO.Path]::GetDirectoryName($ItemPath)
$directoryName = $directory.split('\')[-1]
$zipFile = "$directdoryName.zip"
$destination = "$TargetDir$zipFile"

write-host "Zip destination: $destination"

Compress-Archive $directory\*.* -DestinationPath $destination -Update

#. ./PacOrgSelect.ps1

pac data import --data $destination