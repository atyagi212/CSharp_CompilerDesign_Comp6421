entry
addi   r14,r0,topaddr  % Set stack pointer
main
addi r1,r0,4
sw mainx(r0),r1
lw r1,mainx(r0)
clti r2,r1,5
bz r2,block2
sw -8(r14),r1
addi r1, r0, buf
sw -12(r14),r1
jl     r15,intstr
sw -8(r14),r13
jl     r15,putstr
j endif1
block2
endif1
hlt
mainx      res 4
buf      res 40
