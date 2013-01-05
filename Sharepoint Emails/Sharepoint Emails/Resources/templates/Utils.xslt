﻿<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                 xmlns:user="urn:my-scripts"
                xmlns:d="urn:sharepointemail-context"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <msxsl:script language="C#" implements-prefix="user">
    <msxsl:assembly name="System" />
    <msxsl:using namespace="System" /><![CDATA[public string GetDate(string DateFormat){return DateTime.Now.ToString(DateFormat);}]]>
  </msxsl:script>
  <xsl:template name="eventType">
    <xsl:choose>
      <xsl:when test="./d:EventData[1]/@EventType = 1">added</xsl:when>
      <xsl:when test="./d:EventData[1]/@EventType = 2">modified</xsl:when>
      <xsl:when test="./d:EventData[1]/@EventType = 4">deleted</xsl:when>
      <xsl:otherwise>changed</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="itemType">
    <xsl:choose>
      <xsl:when test="a = b">item</xsl:when>
      <xsl:when test="a = b">file</xsl:when>
      <xsl:otherwise>item(file)</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="itemViewUrl">
    <xsl:value-of select="concat('{SList.DefaultDisplayFormUrl}','?ID=','{SItem.ID}')"/>
  </xsl:template>

  <xsl:template name="getUser">
    <xsl:param name="node" />
    <xsl:choose>
      <xsl:when test="$node/@UserName!=''">
        <xsl:value-of select="$node/@UserName"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$node/@User"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="DUserDisplayName">
    {DUser.LoginName}
  </xsl:template>
  <xsl:template name="SUserDisplayName">
    <xsl:variable name="SUserName" select="d:EventData[1]/@SUserName"/>
    <xsl:choose>
      <xsl:when test="'{SUser.ID}'!=''">
        <a href="/_layouts/userdisp.aspx?ID={SUser.ID}">{SUser.LoginName}</a>
      </xsl:when>
      <xsl:when test="$SUserName !=''">
        <xsl:value-of select="$SUserName"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Someone</xsl:text>
    </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>

  <xsl:template name="Greeting">
    <p>
      <xsl:call-template name="titleTextStyle"/>
      Hello <xsl:call-template name="DUserDisplayName"/>
    </p>
  </xsl:template>
  
</xsl:stylesheet>
