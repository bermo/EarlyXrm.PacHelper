param(
	$ItemPath # path to the *.cdsproj
)

$directory = [System.IO.Path]::GetDirectoryName($ItemPath)

cd $directory

pac solution sync

pac solution version --strategy Solution