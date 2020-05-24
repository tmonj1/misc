# Task and asynchrnous programming in C#

This is a short list of reminders on thead and task in C#.

## 1. Task

A task represetnts *an asynchronous operations.

### 1.1 A task is not necessarily executed on a different thread

A task usually represents an synchronous operation, but not limited to it. 
It is possible to create and run a synchronous task as shown below:

```C#
static void Main(string[] args)
{
    // create a task
    var task = new Task(() => {Console.WriteLine("hello, world");});
    // run it synchronously
    task.RunSynchronously()
}
```

### 1.2 A asynchronous operation is NOT necessarily run on another thread.

In reality, a task almost always represents an asynchronous operation. 
Task is designed for solving asynchronous programming in the first place and
almost always used in this context, so it is perfectly OK to say "a task
represents an asynchronous operation".

Here is a common example of an asynchronous task:

```C#
static async Task Main(string[] args)      // <- use "static async Task Main"
    // create a task
    var task = new Task(() => {Console.WriteLine("hello, world");});
    // run it asynchronously (the task is scheduled to execute)
    task.Start();
    // await it until the task ran to completion
    await task;
}
```

In the above code, when calling task.Start(), the task is put into the queue of the current
`TaskScheduler` (`thread pool task scheduler` by default) for execution. Then, `await task`
awaits the completion of the task.

The asynchronous operation shown above is executed on a different thread. But an asynchronous
operation does not necessarily need a thread. For example, When calling an I/O operation such as
file I/O APIs in a task, it is NOT executed on a different thread (file I/O operation is executed
asynchronously on lower (near H/W) level, where there is no idea of thread).

### 1.3 Task.Run

When creating a new Task, it is convenient to use `Task.Run` method instead of `new Task()`.
The above code example can be rewritten using `Task.Run` method like this:

```C#
static async Task Main(string[] args)
    await Task.Run(() => {Console.WriteLine("hello, world");});
}
```

In the above example, while the task is being executing, the main thread is alive because Main
`await`s the completion of the task.

## 2. Async and Await

### 2.1 Async all the way

You can await asynchronous tasks using `await` keyword and the like (including Task.WhenAll and Task.WhanAny). 
If you use `await` in an async method, the caller has to `await` the async method as well.
Here is an example code. In this code, RunTask1 `await`s RunTask2, and in turn Main `await`s RunTask1.

```C#
class Demo
{
    static async Task RunTask2()
    {
      Console.WriteLine("    RunTask2 started");
      await Task.Run(() =>
      {
        Console.WriteLine("      Inside RunTask2");
      });
      Console.WriteLine("    RunTask2 finished");
    }

    static async Task RunTask1()
    {
      Console.WriteLine("  RunTask1 started");
      await RunTask2();
      Console.WriteLine("  RunTask1 finished");
    }

    static async Task Main(string[] args)
    {
      Console.WriteLine("Main started");
      await RunTask1();
      Console.WriteLine("Main finished");
      return;
    }
}
```

The output of the program is:

```
Main started
  RunTask1 started
    RunTask2 started
      Inside RunTask2
    RunTask2 finished
  RunTask1 finished
Main finished
```

If `await` in Main method is removed (this causes compiler warning because there is no `await` in
an `async` method), the Main method does not `await` the completion of RunTask1, so "Main finished"
can appear before "RunTask1 finished" (and "RunTask2 finished"):

```
Main started
  RunTask1 started
    RunTask2 started
      Inside RunTask2
Main finished
    RunTask2 finished
  RunTask1 finished
```

### 2.2 `await` and `Wait` method (Don't use `Wait`)

Task's `Wait` method is totally different from `await`. `await` is not a syntax sugar of `Wait` method.
`Wait` blocks the thread execution (the thread becomes unresponsive), while `await` only `await`s the
completion of the target task, so its thread is still active.

DO NOT USE `Wait` method, becauase `Wait` method not only cancels the merit of asynchronous programming,
but also can cause a dead lock in UI context. 

`Wait` can cause a dead lock in ASP.NET as well, but NOT in ASP.NET Core, because synchronization context
(request context) was removed in ASP.NET Core and now is context free.

### 2.3 ValueTask

ValueTask is a waitable, so just like `Task`, almost always `await` it. There are some framework methods
returning ValutTask<TResult>, and you can deal with them just like methods returning Task.

## 3. providing async APIs

When you provide asynchronous APIs, abide by the following pattern and guideline:

### 3.1 TAP (Task-based asynchronous pattern)

TAP method signature:

```C#
// return Task when no return value
Task XxxAsync(...)  // <- "Async" suffix
{
    // do something
}

// return Task<TResult> when returning a value of TResult type
Task<TResult> XxxAsync(...)  // <- "Async" suffix
{
    // do something and return a value of TResult type
    return something of type TResult
}
```

### 3.2 guidelines for asynchronous library authors

A library author:
* should only include minimum sync code.
* can immediately return a completed task (i.e. executed synchronouly in this case) if the amount
of work for the task is very small (such as reading bufferred data from input stream).
* see [Correctly Building Asynchronous Libraries in .NET @InfoQ](https://www.infoq.com/articles/Async-API-Design/)

## 4. Threading

### 4.1 ThreadPool and SynchronizationContext

`Task.Run` queues the specified work to run on the ThreadPool. ThreadPool is the default task scheduler
(and the default is almost always used). In ASP.NET Core, requests are processed by ThreadPool threads.
This is different from traditional ASP.NET and ASP.NET MVC which process requests in the SynchonizationContext.
This means that in ASP.NET Core there is no more need to call `ConfigureAwait(false)` to avoid dead lock.

### 4.2 About race condition

No need to call `ConfigureAwait(false)` does not mean that you don't have to care about race condition.
On the contrary, there is more possibility to occur race condition because more than one asynchronous
tasks can run at the same time (In traditional ASP.NET, all tasks from a single request are executed
on the same thread). Developers still need to call `lock` or its equivalent as appropriate.

### 4.3 How threads are asigned to a task?

When a new task is started on the ThreadPool (by calling `Task.Run`, for example), firstly, the new task is
scheduled to execute on the TreadPool. Then, a thread is asigned to the task once a thread is available.

The number of available threads is determined from minimum number and maximum number of ThreadPool threads.
These can be confirmed by calling ThreadPool.GetMinThreads() and Thread.GetMaxThreads().
If there are available threads in the ThreadPool, the new thread is immediately assigned, but if not,
a new thread is assigned after waiting 500ms or so (by creating new thread or recycling a completed one),
unless `LongRunning` is set to `TaskCreationOptions`.

### 4.3 Always use Asyc equivalents?

See [The overhead of async/await in NET 4.5](https://www.red-gate.com/simple-talk/dotnet/net-framework/the-overhead-of-asyncawait-in-net-4-5/). This article says:

> Avoid using async/await for very short methods or having await statements in tight loops
(run the whole loop asynchronously instead). Microsoft recommends that any method that might
take longer than 50ms to return should run asynchronously, so you may wish to use this figure
to determine whether it’s worth using the async/await pattern.

## Other topics

* TPL (Task Parallel Library)
* Unit Testing

## Resources

* [Does async await increases Context switching @StackOverFlow](https://stackoverflow.com/questions/39795286/does-async-await-increases-context-switching)  
a good discussion on the internal behavior of asynchronous methods for beginners.
* [There Is No Thread](https://blog.stephencleary.com/2013/11/there-is-no-thread.html)  
a very clear explanation of internal behavior of async I/O call by Stephen Cleary, the author of "Concurrency in C# Cookbook"
* [Async/Await - Best Practices in Asynchronous Programming @MSDN](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)  
a good article on async programming, also written by Stephen Cleary
* [ASP.NET Core SynchronizationContext](https://blog.stephencleary.com/2017/03/aspnetcore-synchronization-context.html)  
another Stephen Cleary's article on ASP.NET Core Synchronization Context
* [Learn about default TaskScheduler and ThreadPool in .NET to avoid reducing performance of Task.Run drastically](https://getandplay.github.io/2019/05/27/Know-about-default-TaskScheduler-and-ThreadPool-to-avoid-reducing-application-s-performance-drastically/)  
describes how to use ThreadPool for I/O and non-I/O operations.
* [Task-based asynchronous pattern (TAP) @Microsoft](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap?view=netcore-3.1)
* [Correctly Building Asynchronous Libraries in .NET @InfoQ](https://www.infoq.com/articles/Async-API-Design/)
pattens and anti-patterns of async coding from the view points of both a library writer and a library user.
* [Asynchronous programming with async and await @Microsoft](https://docs.microsoft.com/ja-jp/dotnet/csharp/programming-guide/concepts/async/)  
explain how to write async/await code for beginners.
* [ASP.NET Core Performance Best Practices](https://docs.microsoft.com/ja-jp/aspnet/core/performance/performance-best-practices?view=aspnetcore-3.1)
* [Task Parallel Library (TPL)](https://docs.microsoft.com/ja-jp/dotnet/standard/parallel-programming/task-parallel-library-tpl)
* [TPL入門](https://blog.xin9le.net/entry/tpl-intro)
* [Async in depth](https://docs.microsoft.com/en-us/dotnet/standard/async-in-depth)
* [Performance Improvements in .NET Core 3.0](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-core-3-0/)  
contains a detailed discussion about perfromance and threading.
* [ADO: Async all the way down the tubes?](https://stackoverflow.com/questions/49827208/ado-async-all-the-way-down-the-tubes)  
* [The overhead of async/await in NET 4.5](https://www.red-gate.com/simple-talk/dotnet/net-framework/the-overhead-of-asyncawait-in-net-4-5/)