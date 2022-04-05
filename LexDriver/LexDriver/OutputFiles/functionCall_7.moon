entry
addi   r14,r0,topaddr  % Set stack pointer
j main
printTillNum
addi r1,r0,1
sw printTillNumi(r0),r1
gowhile1
lw r1,printTillNumi(r0)
lw r2,printTillNumsize(r0)
clt r3,r1,r2
bz r3,endwhile1
sw -8(r14),r1
addi r1, r0, buf
sw -12(r14),r1
jl     r15,intstr
sw -8(r14),r13
jl     r15,putstr
lw r1,printTillNumi(r0)
addi r2,r1,1
sw printTillNumtemp5(r0),r2
lw r1,printTillNumtemp5(r0)
sw printTillNumi(r0),r1
j gowhile1
endwhile1
hlt
main
addi r1,r0,7
sw mainx(r0),r1
lw r1,mainx(r0)
sw printTillNumsize(r0),r1
jl r15,printTillNum
hlt
printTillNumsize      res 4
printTillNumi      res 4
buf      res 40
printTillNumtemp5      res 4
mainx      res 4
