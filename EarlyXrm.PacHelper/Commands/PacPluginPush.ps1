param(
	$ItemPath # solution file path
)

write-host "solution file path: $ItemPath"

if (Test-Path -Path $ItemPath -PathType Container) {
	write-warning "A valid solution file is required!"
	exit 1
}

$directory = [System.IO.Path]::GetFilenameWithoutExtension($ItemPath)

$query = @"
<fetch><entity name=`"pluginpackage`"><attribute name=`"pluginpackageid`" /><filter><condition attribute=`"name`" operator=`"ends-with`" value=`"$directory`" /></filter></entity></fetch>
"@

$result = pac org fetch --xml $query

$id = $result[4]

write-host $id

pac plugin push --pluginId $id --type Nuget --pluginFile $ItemPath