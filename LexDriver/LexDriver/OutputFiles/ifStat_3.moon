entry
getc r1
sw x(r0),r1
lw r1,x(r0)
ceqi r2,r1,3
bnz r2,block2
lw r1,x(r0)
putc r1
j endif1
block2
endif1
hlt
x      res 4
