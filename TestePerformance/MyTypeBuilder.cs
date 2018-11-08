﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

public static class MyTypeBuilder
{

    public static Type GetType(string className, IDictionary<string, Type> fieldList)
    {
        return CompileResultType(className, fieldList);
    }

    public static object CreateNewObject(Type classType)
    {
        return Activator.CreateInstance(classType);
    }

    public static void SetProperty(object obj, string name, object value)
    {
        PropertyInfo prop = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (null != prop && prop.CanWrite)
            prop.SetValue(obj, value, null);
    }

    public static Type CompileResultType(string className, IDictionary<string, Type> fieldList)
    {
        TypeBuilder tb = GetTypeBuilder(className);
        ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

        // NOTE: assuming your list contains Field objects with fields FieldName(string) and FieldType(Type)
        foreach (var field in fieldList)
            CreateProperty(tb, field.Key, field.Value);

        Type objectType = tb.CreateType();
        return objectType;
    }

    private static TypeBuilder GetTypeBuilder(string className)
    {
        var typeSignature = className;
        var an = new AssemblyName(typeSignature);
        AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null);
        return tb;
    }

    private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
    {
        FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

        PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
        MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
        ILGenerator getIl = getPropMthdBldr.GetILGenerator();

        getIl.Emit(OpCodes.Ldarg_0);
        getIl.Emit(OpCodes.Ldfld, fieldBuilder);
        getIl.Emit(OpCodes.Ret);

        MethodBuilder setPropMthdBldr =
            tb.DefineMethod("set_" + propertyName,
              MethodAttributes.Public |
              MethodAttributes.SpecialName |
              MethodAttributes.HideBySig,
              null, new[] { propertyType });

        ILGenerator setIl = setPropMthdBldr.GetILGenerator();
        Label modifyProperty = setIl.DefineLabel();
        Label exitSet = setIl.DefineLabel();

        setIl.MarkLabel(modifyProperty);
        setIl.Emit(OpCodes.Ldarg_0);
        setIl.Emit(OpCodes.Ldarg_1);
        setIl.Emit(OpCodes.Stfld, fieldBuilder);

        setIl.Emit(OpCodes.Nop);
        setIl.MarkLabel(exitSet);
        setIl.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getPropMthdBldr);
        propertyBuilder.SetSetMethod(setPropMthdBldr);
    }
}
