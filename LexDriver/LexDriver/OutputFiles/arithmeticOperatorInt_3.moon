entry
addi   r14,r0,topaddr  % Set stack pointer
main
getc r1
addi r2,r0,0
addi r3,r0,0
muli r4,r1,0
muli r5,r2,0
add r6,r4,r5
sw mainarr(r6),r1
addi r1,r0,2
muli r2,r1,3
sw maintemp0(r0),r2
addi r2,r0,0
addi r3,r0,0
muli r4,r1,0
muli r5,r2,0
add r6,r4,r5
lw r1,mainarr(r6)
lw r2,maintemp0(r0)
add r3,r1,r2
sw maintemp1(r0),r3
lw r1,maintemp1(r0)
sw mainn(r0),r1
lw r1,mainn(r0)
putc r1
hlt
mainarr      res 100
mainn      res 4
maintemp0      res 4
maintemp1      res 4
