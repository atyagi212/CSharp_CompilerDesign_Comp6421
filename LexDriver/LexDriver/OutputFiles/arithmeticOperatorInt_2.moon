arr[5][5]      res 100
m      res 4
n      res 4
sw m(r0),1
sw n(r0),2
lw r1,0(r0)
lw r2,0(r0)
muli r3,r1,0
muli r4,r2,0
add r5,r3,r4
sw arr(r5),3
lw r1,1(r0)
lw r2,0(r0)
muli r3,r1,4
muli r4,r2,0
add r5,r3,r4
sw arr(r5),4
lw r1,0(r0)
lw r2,1(r0)
muli r3,r1,0
muli r4,r2,4
add r5,r3,r4
sw arr(r5),5
temp0      res 4
lw r1,8(r0)
divi r2,r1,2
sw temp0(r0),r2
lw r1,temp0(r0)
sw n(r0),r1
temp1      res 4
lw r1,1(r0)
addi r2,r1,2
sw temp1(r0),r2
temp2      res 4
lw r1,temp1(r0)
addi r2,r1,3
sw temp2(r0),r2
temp3      res 4
lw r1,temp2(r0)
addi r2,r1,4
sw temp3(r0),r2
lw r1,temp3(r0)
sw n(r0),r1
temp4      res 4
lw r1,1(r0)
subi r2,r1,2
sw temp4(r0),r2
temp5      res 4
lw r1,temp4(r0)
subi r2,r1,3
sw temp5(r0),r2
temp6      res 4
lw r1,temp5(r0)
subi r2,r1,4
sw temp6(r0),r2
lw r1,temp6(r0)
sw n(r0),r1
temp7      res 4
lw r1,1(r0)
muli r2,r1,2
sw temp7(r0),r2
temp8      res 4
lw r1,temp7(r0)
muli r2,r1,3
sw temp8(r0),r2
temp9      res 4
lw r1,temp8(r0)
muli r2,r1,4
sw temp9(r0),r2
lw r1,temp9(r0)
sw n(r0),r1
temp10      res 4
lw r1,1(r0)
lw r2,0(r0)
muli r3,r1,4
muli r4,r2,0
add r5,r3,r4
lw r1,arr(r5)
subi r2,r1,7
sw temp10(r0),r2
temp11      res 4
lw r1,m(r0)
lw r1,0(r0)
lw r2,0(r0)
muli r3,r1,0
muli r4,r2,0
add r5,r3,r4
lw r2,arr(r5)
mul r3,r1,r2
sw temp11(r0),r3
temp12      res 4
lw r1,temp11(r0)
lw r2,temp10(r0)
mul r3,r1,r2
sw temp12(r0),r3
temp13      res 4
lw r1,n(r0)
lw r2,temp12(r0)
add r3,r1,r2
sw temp13(r0),r3
temp14      res 4
lw r1,temp13(r0)
lw r1,0(r0)
lw r2,1(r0)
muli r3,r1,0
muli r4,r2,4
add r5,r3,r4
lw r2,arr(r5)
div r3,r1,r2
sw temp14(r0),r3
lw r1,temp14(r0)
sw n(r0),r1
