using System;

namespace JJMasterData.Commons.Background.Queue;

public sealed class BackgroundJobQueueFullException(string message) : InvalidOperationException(message);