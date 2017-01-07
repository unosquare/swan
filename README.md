[![Build Status](https://travis-ci.org/unosquare/swan.svg?branch=master)](https://travis-ci.org/unosquare/swan)
[![Build status](https://ci.appveyor.com/api/projects/status/063tybvog8mb1sic/branch/master?svg=true)](https://ci.appveyor.com/project/geoperez/swan/branch/master)
[![NuGet version](https://badge.fury.io/nu/Unosquare.Swan.svg)](https://badge.fury.io/nu/Unosquare.Swan)
[![Coverage Status](https://coveralls.io/repos/github/unosquare/swan/badge.svg?branch=master)](https://coveralls.io/github/unosquare/swan?branch=master)
# <img src="https://github.com/unosquare/swan/raw/master/swan-logo-32.png"></img> SWAN: Stuff We All Need

*:star: Please star this project if you find it useful!*

SWAN stands for Stuff We All Need

Repeating code and reinventing the wheel is generally considered bad practice. At Unosquare we are committed to beautiful code and great software. 
Swan is a collection of classes and extension methods that we and other good developers have developed and evolved over the years. We found ourselves copying and pasting 
the same code for every project every time we started it. We decide to kill that cycle once and for all. This is the result of that idea.
Our philosophy is that SWAN should have no external dependencies, it should be cross-platform, and it should be useful.

NuGet Installation:
-------------------
```
PM> Install-Package Unosquare.Swan
```

## What's in the library

In this section we present the different components that are available in the Swan library. Please keep in mind that everything in the library is opt-in.
Swan won't force you to use any of its components, classes or methods.

### The `CurrentApp`

`CurrentApp` provides properties and methods that provide information about 

### The `Terminal`

Many times we find ourselves implementing `Console` output code as some NLog or Log4Net logger or adapter, especially 
when writing console applications, daemons and windows services. We also tend to write `Console` code for reading user 
input because it can't be some logger or adapter. And then you have the `System.Diagnostics.Debug` class to write 
to the debugger output. And finally, all your `Console` user interaction looks primitive and unprofessional. In other 
words, you end up with 3 things you are unsure of how to put together in the different configurations and runtime environments:
`Console`, `Debug` and some logging mechanism. In return you have placed unintended logging code, `Console` code, and `Debug` 
code everywhere in your application and it makes it look silly, bloated and written by an amateur.  

The Swan `Terminal` is __all__ of the following:
- Console Standard Output Writer
- Console Standard Error Writer 
- Debug Writer
- Console Standard Input Reader
- Log message forwarder

It is also very easy to use, it's thread-safe, and it does not rquire you to learn anything new. In fact it simplifies logging
messages and diplaying `Console` messages by providing `string` extension methods.

#### Example 1: Writing to the Terminal

This only writes messages out to the `TerminalWriters` if they are avialble. In practice, we typically **DO NOT** use
the `Write` and `WriteLine` methods but they are provided for convenience, extensibility and customization. Please note
that these methods do not forward messages as logging events and therefore whatever is written via these methods
will not show up in you logging subsystem.

```csharp
// The simplest way of writing a line of text -- equivalent to `Console.WriteLine`:
Terminal.WriteLine($"Hello, today is {DateTime.Today}");

// A slightly better way using extension methods:
$"Hello, today is {DateTime.Today}".WriteLine();

// Now, let's add some color:
$"Hello, today is {DateTime.Today}".WriteLine(ConsoleColor.Green);

// Write it out to the debugger as well!
$"Hello, today is {DateTime.Today}".WriteLine(ConsoleColor.Green, TerminalWriters.StandardOutput | TerminalWriters.Diagnostics);

// You could have also set the color argument to null and just use the configured default
$"Hello, today is {DateTime.Today}".WriteLine(null, TerminalWriters.StandardOutput | TerminalWriters.Diagnostics);
```

#### Example 2: Basic Logging

This is where `Terminal` really shines. Instead of using the `Write` and `WriteLine` methods, you can use the 
methods that are intended for logging. These methods have different purposes and distinct functionality. Please
refer to the example below and its comments.

```csharp
$"Hello, today is {DateTime.Today}".Info();
```

#### Example 3: Forwarding Logging Messages

Suppose you have various calls to `Terminal`'s logging methods such as `Info()`, `Warn()`, `Error()`, `Trace()`
and `Debug()`. You wish to forward those messages to a logging subsystem in addition to using the `Console`'s
standard output and standard error, and the built-in diagnostics output. All you have to do is subscribe to the
Terminal's `OnLogMessageReceived` event. The event arguments of this event provide useful properties that you
can piece together to send your logging messages directly to the Logging subsystem in your application.

#### Example 4: Configuring Output

Swan's `Terminal` provides both, flexibility and consistency for all of its output. While it will pick the most
common defaults for a given build or runtime scenario, you are able to modify such defaults and adjust them to your
liking. You can change the output colors,  

#### Example 5: User Interaction

The Swan `Terminal` would not be complete without a way to read user input. The good news is
that `Terminal` can create decent-looking user prompts if a very convenient way.

```csharp
// TODO: These examples need work.

var lineResult = Terminal.ReadLine();

var numberResult = Terminal.ReadNumber();

var promptResult = Terminal.ReadPrompt();

var keyResult = Terminal.ReadKey();
``` 

#### Example 6: Other Useful Functions

Swan's `Terminal` also provides additional methods to accomplish very specific tasks. Given the fact that `Terminal`
is an asynchronous, thread-safe output queue, we might under certain situations require all of the output queue to be written
out to the `Console` before the program exits. For example, when we write a console application that requires its usage
to be fully printed out before the process is terminated. In these scenarios we use `Terminal.Flush` which blocks
the current thread until the entire output queue becomes empty.