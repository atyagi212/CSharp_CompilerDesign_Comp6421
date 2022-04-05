entry
addi   r14,r0,topaddr  % Set stack pointer
main
addi r1,r0,3
sw maini(r0),r1
gowhile1
lw r1,maini(r0)
clti r2,r1,9
bz r2,endwhile1
sw -8(r14),r1
addi r1, r0, buf
sw -12(r14),r1
jl     r15,intstr
sw -8(r14),r13
jl     r15,putstr
lw r1,maini(r0)
addi r2,r1,1
sw maintemp2(r0),r2
lw r1,maintemp2(r0)
sw maini(r0),r1
j gowhile1
endwhile1
hlt
maini      res 4
buf      res 40
maintemp2      res 4
