entry
addi   r14,r0,topaddr  % Set stack pointer
main
getc r1
addi r2,r0,0
addi r3,r0,0
addi r4,r0,0
muli r5,r1,0
muli r6,r2,0
muli r7,r3,0
add r8,r5,r6
add r9,r7,r8
sw mains(r9),r1
addi r2,r0,0
addi r3,r0,0
addi r4,r0,0
muli r5,r1,0
muli r6,r2,0
muli r7,r3,0
add r8,r5,r6
add r9,r7,r8
lw r1,mains(r9)
putc r1
hlt
mains      res 96
