ForEach ($proj in "ThinMvvm","ThinMvvm.Logging","ThinMvvm.WindowsPhone") {
  nuget pack $proj/$proj.csproj -Symbols -Prop Configuration=Release
}