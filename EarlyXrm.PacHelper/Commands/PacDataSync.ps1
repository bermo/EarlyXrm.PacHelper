param(
	$ItemPath # data schema file path
)

write-host "data schema file path: $ItemPath"

if (Test-Path -Path $ItemPath -PathType Container) {
	write-warning "A valid data file is required!"
	exit 1
}

$fileName = [System.IO.Path]::GetFileNameWithoutExtension($ItemPath)
$directory = [System.IO.Path]::GetDirectoryName($ItemPath)
$zipFile = "$fileName.zip"

write-host $fileName
write-host $directory

pac data export -sf $ItemPath -o -df $zipFile
Expand-Archive -path $zipFile -DestinationPath $directory -Force

del $zipFile