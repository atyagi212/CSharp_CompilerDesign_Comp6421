entry
addi   r14,r0,topaddr  % Set stack pointer
main
getc r1
sw mainn(r0),r1
lw r1,mainn(r0)
putc r1
hlt
mainn      res 4