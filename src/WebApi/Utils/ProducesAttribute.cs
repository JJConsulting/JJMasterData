using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.WebApi.Utils;

public sealed class ProducesAttribute<T>() : ProducesAttribute(typeof(T));