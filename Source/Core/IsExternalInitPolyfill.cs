// Polyfill für C#-9 init-only-Properties / records unter net472.
// `System.Runtime.CompilerServices.IsExternalInit` ist erst in .NET 5+ vorhanden.
// `internal` → pro Assembly scoped, keine Mod-Kollision möglich.
// Kann entfernt werden wenn wir auf ein Target-Framework mit eingebauter Unterstützung wechseln.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
