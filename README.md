# csharp-dynamic-performance

1. Scenary
I had to get a huge ammount of data from elasticsearch to do some custom sorting, the data there is dynamic so I couldn't create a class where I would put the data from it. That caused some problems of performance.

2. Experiment
Seeing how v8 handles javascript dynamic object creating I got the idea to do this with c#. Using some classes on Reflection namespace I was able to create a new class at runtime. That allowed me to reduce memory costs almost 3 times.
