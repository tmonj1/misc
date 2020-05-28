# Web performance tuning

## 1. Targeting and Prioritize

1. Determine in what context (situation) your app becomes slow using the following matrix:

    |           | Huge Data   | Small Data              |
    | :-------- | :---------- | :---------------------- |
    | High load | thruput     | app server              |
    | Low load  | data access | design / implementation |

    List up the situation if the above matrix does not work for you. For example, in some cases, app slows down in the context such as:
    * when accessed for the first time
    * when accessed by IE 

2. Determine what functionality/window/operation is slow.
3. Combine the results of 1 and 2, and you will get a list of combinations of functionality/window/operation and context.
4. Prioritize the list items and sort them from the most important to the least important. The top item on the list is the one you should tacke first.

## 2. Measure and Analyze

1. Make a performance measurement plan carefully. The plan should cover all the measurement points in the target context, which is enough to analyze the cause of low performance.
2. Measure the current performance according to the plan. There might need some additional work (design and implementation) for instrumentation or introducing new tools.
3. Analyze the result data and determine what approach you take. There are two major ways to improve the response time:

   1. improve the real response time
   2. improve perceived response time

   From the view point of design, there are many means for performance tuning such as:

   * off loading (introducing reverse proxy etc.)
   * caching
   * code flow optimization
   * redesign
  
## 3. Fix and/or Improve

1. Fix or improve your code
2. Carefully review the fix
3. Measure the performance in the same condition as the original performance measurement and confirm your fix does improve the performance.

## Resources

### ExtJS performance tuning
* [Extjs - Performance tuninig](http://jsext.blogspot.com/2016/01/extjs-performance-tuning.html)
* [Performance Optimization for Layout Runs](https://www.sencha.com/blog/performance-optimization-for-layout-runs/)