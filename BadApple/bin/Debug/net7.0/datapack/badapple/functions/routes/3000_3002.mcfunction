#> badapple:routes/3000_3002
# Auto-generated by Kaka // 2022/12/4 下午3:59:35

execute if entity @e[tag=BadApple.Main,scores={BadApple.Time=3000}] run function badapple:frames/3000
execute if entity @e[tag=BadApple.Main,scores={BadApple.Time=3001..3002}] run function badapple:routes/3001_3002
