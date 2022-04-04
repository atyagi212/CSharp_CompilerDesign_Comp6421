entry
addi   r14,r0,topaddr  % Set stack pointer
printTillNumsize res 4
addi r1,r0,1
sw i(r0),r1
gowhile1
lw r1,i(r0)
lw r2,size(r0)
clt r3,r1,r2
bnz r3,endwhile1
sw -8(r14),r1
addi r1, r0, buf
sw -12(r14),r1
jl     r15,intstr
sw -8(r14),r13
jl     r15,putstr
lw r1,i(r0)
addi r2,r1,1
sw temp0(r0),r2
lw r1,temp0(r0)
sw i(r0),r1
j gowhile1
endwhile1
addi r1,r0,10
sw x(r0),r1
lw r1,x(r0)
sw printTillNump1(r0),r1
jl r15,printTillNum
hlt
i      res 4
buf      res 40
temp0      res 4
x      res 4
