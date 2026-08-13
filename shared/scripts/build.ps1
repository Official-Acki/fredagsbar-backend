$decision = $Host.UI.PromptForChoice(
	"Make sure you are executing this from outside the scripts folder. Inside the shared folder.",
	"Are you?",
	("&Yes", "&No"),
	1
)

if ($decision -eq 1) {
	exit
}

dotnet build -c Release

cp bin/Release/Fredagsbar.Shared.*.nupkg ../local-nuget-feed
