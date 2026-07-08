The content of this project is a "follow up" to a doc exploring the feature of extending the boundary context to implement a special efficient version of Dynamic Boundary Context: https://zenodo.org/records/21175352
So this answer positively to the fact that in Sharpino you can implement an optimized version of DBC.
Is is optimized as it doesn't imply reading historical events on the Event store do do conditional append, but rather reading some cache available state.

Second question is: how efficient is pure appending events (compared to UmaDb)?
In Sharpino you normally don't append events or initial states directly but rather delegate this to the CommandHandler passing initial instances or commands to be executed.

Adding new objects (initial states) means creating initial snapshots and feeding the cache. So the object are immediately available in cache.
Adding new events means sending a command and the framework will generate the events, store the events, update the aggregate in cache and eventually publish the events.






