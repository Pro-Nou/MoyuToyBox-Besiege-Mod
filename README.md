# MoyuToyBox  
Added some fun blocks into Besiege  

# Previews  
注意：视频中的卡顿是由github播放器导致的，暂时没有找到解决办法。  
      如果想查看流畅的视频请下载到本地。  
NOTE: Freezes in videos are caused by github's player, I'm not sure how to solve it.   
      If you want to view a smooth version, please download it.  

# (鼠标炮控)SmartTurret
https://user-images.githubusercontent.com/74658051/180628818-7d63818f-c812-47db-a6b3-fe4d7d11bd71.mp4

该零件会尝试在其自身XOZ平面内指向主摄像机指向的点。  
该行为是通过Joint而不是LookAt()方法实现的，因此具有更好的物理效果。  
This block will try to aim at the point that main camera is looking at via it's local XOZ plane.  
Such a behaviour is achieved by Joint but not LookAt(), which could provide better physic effects.  

# (鱼雷/等离子体)TorpedoLauncher
https://user-images.githubusercontent.com/74658051/180628836-757a984e-9414-417f-a446-491b306c3b51.mp4

发射鱼雷或等离子体，所有特效支持自定义颜色。  
launch a torpedo or plasma ball. All EFXs support custom colors.  

# (导弹)MissileLauncher
https://user-images.githubusercontent.com/74658051/180628831-d8a31502-4ee1-4048-8c5d-9665628634d4.mp4

发射指向主摄像机指向的点的导弹，所有特效支持自定义颜色。  
launch missiles that aim at the point that main camera is looking at. All EFXs support custom colors.  

# (激光)LaserLauncher
https://user-images.githubusercontent.com/74658051/180628826-318b20a8-2800-456e-bf73-dff1f31fdda6.mp4

对指向的刚体施加爆炸力，力度随激光长度增加而衰减。  
所有特效支持自定义颜色， 激光内部和外部颜色可分别调整。  
Add explosion force to rigidbody, the power of explosion force will decrease by increasing of laser length.    
All EFXs support custom colors. Laser's inner and outer colors can be adjusted separately.  

# (光弹/霰弹)BeamRifle
https://user-images.githubusercontent.com/74658051/180628828-058d29b4-3dab-412d-bc1d-f83ff7c97a73.mp4

具有光弹和霰弹两种模式。  
光弹会继承零件的运动速度，对击中的刚体施加爆炸力，没有溅射。  
霰弹对范围内实体施加爆炸力，通过一16面锥形MeshCollider定义的触发器实现。  
所有特效支持自定义颜色， 光弹内部和外部颜色可分别调整。  
Could be a beam cannon or shotgun.  
As for beam cannon, bullets will inherit speed of the block, and add explosion force to hitted rigidbody, without splash damage.  
As for shotgun, it will add explosion force to rigidbodys in range. Achieved by a trigger that defined by a 16-sided tapered MeshCollider.   
All EFXs support custom colors. Beam bullet's inner and outer colors can be adjusted separately.  

# (光剑)BeamSaber
https://user-images.githubusercontent.com/74658051/180628830-38e26e15-fa08-47f8-8394-7d78e7a03e2d.mp4

对接触到的刚体施加沿剑身方向的力。  
刀光通过动态修改Mesh的顶点组实现。  
所有特效支持自定义颜色。  
Add force in direction of the blade to touched rigidbodys.  
Blade trail is achieved by dynamically modifying vertexs of Mesh.  
All EFXs support custom colors.  

# (实体剑)MetalSword
https://user-images.githubusercontent.com/74658051/180628822-f27c9355-4401-4cd4-838c-d08de7f1bb4d.mp4

对接触到的刚体施加垂直于刀片的力，运动速度越快力越大。  
所有特效支持自定义颜色。  
Add force that normal to the blade to touched rigidbodys.  
All EFXs support custom colors. The power of force will increase by block's speed.   

# (护盾)EnergyShield
https://user-images.githubusercontent.com/74658051/180628827-c6417f18-f77a-495c-8155-602ffaf2b517.mp4

通过零件的BlockHealth()实现。支持多种形状。  
通过动态更新坐标而不是Joint来连接以隔断力的传递。  
每0.25秒回复自定义数值的BlockHealth，当BlockHealth归0时护盾失效。视频中使用了几种不同的恢复速率以更直观地观察效果。
所有特效支持自定义颜色。  
Achieved by BlockHealth(). Supports several custom shapes.  
Connect with other blocks by dynamically modifying transform.position but not Joint to resist transmission of force.  
Recharge BlockHealth with custom value each 0.25 second. The shield will deactivate once BlockHealth becomes 0. Several different recovery rates are used in the video to see the effect more intuitively.  
All EFXs support custom colors.  

# (LED灯块)LedBlock
https://user-images.githubusercontent.com/74658051/180628833-f184ea7e-bfd4-466f-8c83-36c3c64d057a.mp4

支持多种形状的高亮零件，用法类似MeshMod。  
在Start()阶段直接将自身设置为其他零件的Child而不是使用Joint连接以减少资源开销。  
所有特效支持自定义颜色。  
A highlight block that supports several custom shapes. It is similar to MeshMode.  
Connect to other blocks by set itself as child during Start() but not using Joint to reduce resource cost.  
All EFXs support custom colors.  

# (飘带&尾迹)TrailFXBlock
https://user-images.githubusercontent.com/74658051/180628823-dc31bfe4-afaa-4d82-802a-961387d69bb9.mp4

支持多种纹理的飘带效果。  
飘带通过动态修改LineRenderer的顶点组实现。可受重力影响。  
视频中距离摄像机最远的为TrailRenderer，对比以更直观的查看飘带与尾迹的区别。  
所有特效支持自定义颜色。 
Ribbon effect that supports several custom textures.  
Ribbon is achieved by dynamically modifying vertexs of LineRenderer. Can be affected by gravity.  
In the video, the farthest EFX from main camera is TrailRenderer, differences between ribbon and trail can be viewed more intuitively by comparison.  
All EFXs support custom colors.  

# (粒子)ParticleFXBlock
https://user-images.githubusercontent.com/74658051/180628825-e47fbe49-3ac8-4af2-901e-357b90427be2.mp4

几乎可以修改ParticleSystem的所有属性。  
相比于SpecialEffectMod中的ParticleEmitter，该零件可以修改粒子缩放模式和粒子渲染模式，从而实现基于模型的粒子，根据速度方向缩放的粒子等效果。  
所有特效支持自定义颜色。 
Almost all properties of ParticleSystem can be modified.  
Compared with ParticleEmitter in SpecialEffectMod, this block could modify particle scaling mode and particle rendering mode. Thus, particle effects such as model-based particles, particles scaled according to the speed direction, etc. could be achieved.  
All EFXs support custom colors.  

# RGB
https://user-images.githubusercontent.com/74658051/180632038-853d5c67-7287-4d11-af90-60b2e36e94fc.mp4

每一个零件都具有的通用效果，启用后高光部分会变为RGB渐变色。  
为节省资源，该效果由一个SingleInstance进行HSV到RGB的转换，各零件每帧读取该单例的返回值即可，因此所有零件的RGB渐变是同步的。  
A common component that every blocks has. When enabled, the highlight part will become RGB gradient.  
For reducing resource cost, a SingleInstance transform HSV to RGB, then return the RGB value to each blocks. Thus RGB gradients for all blocks are synchronized.  
