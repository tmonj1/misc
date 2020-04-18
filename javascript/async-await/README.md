# Undestanding async and await

## What is this?

The program demonstrates how different types of asynchronous code is executed;
plain asynchronous method, promise and  promise with await. Plus, this also
shows how async/await works without using async/await, instead by using
generator to mimic it.

## async and await

"await" waits for the resolution of the target promise, and "async" keyword is
prepended to the function in which "await" is used. As an example, take a
look at the code below:

```JavaScript:
async function foo() {
  console.log("calling bar");
  await bar(); // bar() is an asynchronous method which returns a promise.
  console.log("bar called.");
}

console.log("calling foo.");
foo();
console.log("foo called.");
```

When you execute the code above, the output is as follows:

```
calling foo.
calling bar.
foo called.
bar called.
```

Because **await** waits for bar() to get resovled and returns to the caller
immediately after executing bar() in the middle of foo method, "foo called." is
output before "bar called." which is output when the promise gets resovled and
the next line of **await** gets reactivated and executed.

If there was no **await** in this code (and **async**, too), "bar called" comes
before "foo called."

So, using **await** makes it possible to execute lines of code including
asynchronous call in the same method **sequentially**. This makes code more
readable and easy to write.

A function in which **await** is used must be declared as **async** function.
This is because such a function is different from a ordinary function in terms
of that it can return in the middle of execution, and then can be reactivated
from the point where it left last time. In other word, such a function has more
than one entry point.

The behavior of **await** is just like "yield" statement in a generator.
Actually, it is possible to rewrite **await** using generator.

## Usage of program

The program demonstrates how different types of asynchonous calls are executed
including:

1. simple asynchronous method call
1. promise
1. promise + await
1. generator (rewrite version of await + promise using generator)

You can understand how the asyncronous code is executed by comparing source
code and the output of program.

To use the program, type `node index.js type_of_call`, where type_of_call
is one of these: `async`, `promise`, `promise-await` and `generator`. Or, type
`node index.js -h` for help.

```bash:
# show help
$ node index.js -h
Usage: node index.js -h|async|promise|promise-await|generator
# 1. executes simple async method
$ node index.js async
:
: (omitted)
:
```

