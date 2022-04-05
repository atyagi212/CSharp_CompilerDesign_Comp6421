entry
addi   r14,r0,topaddr  % Set stack pointer
main
getc r1
sw maina(r0),r1
addi r1,r0,3
sw mainb(r0),r1
addi r1,r0,2
sw mainc(r0),r1
lw r1,mainb(r0)
lw r2,mainc(r0)
mul r3,r1,r2
sw maintemp3(r0),r3
lw r1,maina(r0)
lw r2,maintemp3(r0)
add r3,r1,r2
sw maintemp4(r0),r3
lw r1,maintemp4(r0)
sw mainn(r0),r1
lw r1,mainn(r0)
putc r1
hlt
maina      res 4
mainb      res 4
mainc      res 4
mainn      res 4
maintemp3      res 4
maintemp4      res 4
