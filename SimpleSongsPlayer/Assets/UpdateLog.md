﻿大家好哇，最近闲来无事把这款应用重写了，在此之前这款应用算是我最失败的作品了  
原因是它很容易崩溃，一旦崩起来就需要重装，而且重装还不一定能好的那种🤣  
这次重写基本上解决了这个问题，同时扫描性能比以前提升百倍有余  
不过加载文件的性能倒是没啥改变，还是那么慢，这个问题就以后再说吧

如果真的经常需要一次性播放大量文件的，可以试试我的另外一款应用“简易有声书播放器”
这款应用我做了专门的加载性能优化，绝对可以做到即点即播

但是这次重写还是有个遗憾的，就是不再支持手机，不过界面适配还是做了一些的  
不支持的原因是我提升了sdk的版本，以便使用winui 2.7 ui框架
ok，就说到这吧

v3.0.0 更新日志

1. 基本杜绝崩溃情况，即使有崩溃，重启应用应该就能恢复
2. 扫描性能提升百倍
3. 现在只会在启动时扫描一次音乐库
4. 大幅简化界面，取消原来的专辑视图
5. sdk版本提升至 19041