d1=read.delim("out1.txt")
d3=read.delim("out3.txt")

plot(d1$x, d1$y, main="start with working lane", xlab="East (m)", ylab="North (m)")
savePlot("route1.png", type="png")

plot(d3$x, d3$y, main="start with headland", xlab="East (m)", ylab="North (m)")
savePlot("route3.png", type="png")

plot(d1$c, d1$distance_traveled, xlab="time (s)", ylab="distance (m)")
points(d3$c, d3$distance_traveled, col="red")
legend("topleft"
     , c("start with working lane", "start with headland")
     , col=c("black","red")
     , pch=16)
savePlot("t_distance.png", type="png")

plot(d1$c, d1$time_to_go, xlab="time (s)", ylab="time-to-go (s)")
points(d3$c, d3$time_to_go, col="red")
legend("topright"
     , c("start with working lane", "start with headland")
     , col=c("black","red")
     , pch=16)
savePlot("t_time-to-go.png", type="png") 
