using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Godot;
using Microsoft.Extensions.Logging;

public partial class Log : Node, ILogger, ILoggerProvider {
	public enum Level {
		CRIT,
		DEBUG,
		ERROR,
		INFO,
		TRACE,
		UNK,
		WARN,
	}

	public static Log Instance;

	private readonly Channel<object> _prints = Channel.CreateUnbounded<object>();

	private readonly string _projectdir = DetectProjectPath();

	public int Queued = -1;

	public IDisposable BeginScope<TState>(TState state) where TState : notnull {
		return default!;
	}

	bool ILogger.IsEnabled(LogLevel logLevel) {
		return true;
	}

	void ILogger.Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception exception,
		Func<TState, Exception, string> formatter
	) {
		var level = logLevel switch {
			LogLevel.Critical => Level.CRIT,
			LogLevel.Debug => Level.DEBUG,
			LogLevel.Error => Level.ERROR,
			LogLevel.Information => Level.INFO,
			LogLevel.None => Level.UNK,
			LogLevel.Trace => Level.TRACE,
			LogLevel.Warning => Level.WARN,
			_ => Level.UNK,
		};
		var message = $"{formatter(arg1: state, arg2: exception)}";
		var isbad = level switch {
			Level.ERROR => true,
			Level.CRIT => true,
			_ => false,
		};
		if (isbad) {
			_ = exception != null ? exception.Message : "";
			var stackTrace = new StackTrace();
			message += $": {exception?.Message}\n{exception?.StackTrace ?? stackTrace.ToString()}";
		}
		this.Print(
			level: level,
			message: message
		);
	}

	public ILogger CreateLogger(string categoryName) {
		return Instance;
	}

	public override void _Ready() {
		Instance = this;
		this.Print(message: $"Project Directory: {this._projectdir}");
		this.Print(message: "Logging Ready");
	}

	public void Print(
		object message,
		Level level = Level.INFO,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0
	) {
		var filePath = sourceFilePath.Replace(oldValue: this._projectdir, newValue: "");
		var outmsg = $"[{level}][{filePath}:{sourceLineNumber}][{memberName}]: {message}";
		//var _ = this._prints.Writer.WriteAsync(item: outmsg);
		GD.Print(outmsg);
	}

	public void Error(
		Exception e,
		string message = "",
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0
	) {
		message = message == ""
			? $"{e.GetType().FullName}: {e.Message}\n{e.StackTrace}"
			: message;
		this.Print(
			level: Level.ERROR,
			message: message,
			memberName: memberName,
			sourceFilePath: sourceFilePath,
			sourceLineNumber: sourceLineNumber
		);
	}

	public override void _Process(double delta) {
		if (this._prints.Reader.CanCount) {
			this.Queued = this._prints.Reader.Count;
		}
		if (this._prints.Reader.TryRead(item: out var message)) {
			GD.Print(what: message);
		}
	}

	private static string DetectProjectPath([CallerFilePath] string sourceFilePath = "") {
		// HINT: this is still here in case we do path renames again. Any better ideas?
		var offset = 0;
		var upper = sourceFilePath.IndexOf(value: "Core") - offset;
		return sourceFilePath[..upper];
	}

	public void Print(
		string message,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0
	) {
		this.Print(
			message: message,
			level: Level.INFO,
			memberName: memberName,
			sourceFilePath: sourceFilePath,
			sourceLineNumber: sourceLineNumber
		);
	}

	public void Print(
		StackTrace stackTrace,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0
	) {
		this.Print(
			message: $"stacktrace:\n{stackTrace}",
			level: Level.INFO,
			memberName: memberName,
			sourceFilePath: sourceFilePath,
			sourceLineNumber: sourceLineNumber
		);
	}

	public void Print(
		Exception e,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0
	) {
		this.Print(
			message: $"exception: {e.Message}\n{e.StackTrace}",
			level: Level.INFO,
			memberName: memberName,
			sourceFilePath: sourceFilePath,
			sourceLineNumber: sourceLineNumber
		);
	}
}