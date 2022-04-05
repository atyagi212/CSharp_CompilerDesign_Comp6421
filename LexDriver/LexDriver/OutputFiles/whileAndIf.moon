entry
addi   r14,r0,topaddr  % Set stack pointer
main
addi r1,r0,7
sw mainnum(r0),r1
addi r1,r0,1
sw maini(r0),r1
gowhile1
lw r1,maini(r0)
clti r2,r1,10
bz r2,endwhile1
lw r1,mainnum(r0)
lw r2,maini(r0)
ceq r3,r1,r2
bz r3,block2
sw -8(r14),r1
addi r1, r0, buf
sw -12(r14),r1
jl     r15,intstr
sw -8(r14),r13
jl     r15,putstr
j endif1
block2
endif1
lw r1,maini(r0)
addi r2,r1,1
sw maintemp6(r0),r2
lw r1,maintemp6(r0)
sw maini(r0),r1
j gowhile1
endwhile1
hlt
mainnum      res 4
maini      res 4
buf      res 40
maintemp6      res 4
