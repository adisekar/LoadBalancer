# LoadBalancer
Custom LoadBalancer Implementation using .Net Core 3.1 / C#

TO Run
1. Open Servers Solution, and run. It should run as project, and have 3 servers running in 3 different ports.
2. Open LoadBalancerService Solution, and run. It has the loadbalancer service running in port 4000.
3. Import the Postman collection, and it has 5 different client calls, name Get Client 1 .. 5 - LB

Highlights
1. Load Balancer routes the traffic to differnt servers based on number of active connections in each server, and picks the server with least connections. 
2. Active connection for client to a server is 30 seconds.
3. During the active session, client is sticky session to the same server.
4. New Servers can be added by Service Discovery, adding to appSettings.json. In real app, can be done by Consistent Hashing

To Do-
1. Use MinHeap to keep track of least active connections. C# does not have an implementation for PriorityQueue. We need to build a custom Minheap.
  As this will improve performance to get least active server in O(1), and updating the server is O(log n). Currently it is O(n log n) where n is number of active servers.
2. Add Health checks on each server, so if server is down, Load balancer does not pick it.
3. Active connections in each server and which client is connected to which server information is stored in memory. Need to be stored in caching server. So we can have 
  Load balancer service running as a cluster as well. 
4. Current implementation is not Thread safe. Real App with data from cache like Redis, can be made to be Thread safe using Locks and Concurrent collections. 

