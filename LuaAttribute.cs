/*
 * @Description 转换为lua table 字段重新命名
 * @Author SunShubin
 * @Time 2018-03-12
 */

using System;
[AttributeUsage(AttributeTargets.All)]
public class LuaAttribute : Attribute {
    public bool useDefaultName;
    public string luaName;
    public LuaAttribute(string luaName,bool useDefaultName = false){
        this.useDefaultName = useDefaultName;
        if(!useDefaultName){
            this.luaName = luaName;
        }
    }
	
}
