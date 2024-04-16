using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Logging;

public abstract class LoggerBuffer(int maxSize) : BufferBase<LogMessage>(maxSize);