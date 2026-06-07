<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

    <xsl:template match="/">

        <html>
        <head><font size="5">LM data --- Stylesheet sample</font><br /></head>

        <body>

            <xsl:apply-templates select="Ophthalmology/Measure/@type" />

        </body>

        </html>

    </xsl:template>

    <!--============================================================================================-->
    <xsl:template match="Ophthalmology/Measure/@type">

        <br />
        <strong><xsl:text>Measurement conditions</xsl:text></strong>

        <xsl:call-template name="LM-Conf">
            <xsl:with-param name="Conf" select="./.." />
        </xsl:call-template>

        <br />
        <xsl:if test="../LM/S">
            <strong><xsl:text>Single</xsl:text></strong>

            <xsl:call-template name="LM-SRL">
                <xsl:with-param name="SRL-value" select="../LM/S" />
            </xsl:call-template>

            <br />
        </xsl:if>

        <xsl:if test="../LM/R">
            <strong><xsl:text>Right side</xsl:text></strong>

            <xsl:call-template name="LM-SRL">
                <xsl:with-param name="SRL-value" select="../LM/R" />
            </xsl:call-template>

            <br />
        </xsl:if>

        <xsl:if test="../LM/L">
            <strong><xsl:text>Left side</xsl:text></strong>

            <xsl:call-template name="LM-SRL">
                <xsl:with-param name="SRL-value" select="../LM/L" />
            </xsl:call-template>

            <br />
        </xsl:if>

        <xsl:if test="../PD">
            <strong><xsl:text>PD</xsl:text></strong>

            <xsl:call-template name="PD">
                <xsl:with-param name="PD-value" select="../PD" />
            </xsl:call-template>

            <br />
        </xsl:if>

        <xsl:if test="../NIDEK">
            <strong><xsl:text>NIDEK any data</xsl:text></strong>
            <br />

            <xsl:call-template name="NIDEK">
                <xsl:with-param name="NIDEK-value" select="../NIDEK" />
            </xsl:call-template>
            <br />
        </xsl:if>

        <br />
        <font size="5">Common</font>

        <xsl:call-template name="Common">
            <xsl:with-param name="comm-value" select="../../Common" />
        </xsl:call-template>

    </xsl:template>

    <!--==========================    LM measurement conditions    ==========================-->
    <xsl:template name="LM-Conf">
        <xsl:param name="Conf" />

        <table border="1" Cellspacing="0">
            <tr align="center">
                <td bgcolor="mediumseagreen" width="150"><font color="#ffffff">Measure mode</font></td>
                <td width="150"><xsl:value-of select="$Conf/MeasureMode" /><xsl:value-of select="$Conf/MeasureMode/@unit" /></td>
            </tr>
            <tr align="center">
                <td bgcolor="mediumseagreen" width="150"><font color="#ffffff">Diopter step</font></td>
                <td width="150"><xsl:value-of select="$Conf/DiopterStep" /><xsl:value-of select="$Conf/DiopterStep/@unit" /></td>
            </tr>
            <tr align="center">
                <td bgcolor="mediumseagreen" width="150"><font color="#ffffff">Axis step</font></td>
                <td width="150">
                    <xsl:value-of select="$Conf/AxisStep" /><xsl:if test="$Conf/AxisStep/@unit[. = 'deg']"><xsl:text>°</xsl:text></xsl:if>
                </td>
            </tr>
            <tr align="center">
                <td bgcolor="mediumseagreen" width="150"><font color="#ffffff">Cylinder mode</font></td>
                <td width="150"><xsl:value-of select="$Conf/CylinderMode" /></td>
            </tr>
            <tr align="center">
                <td bgcolor="mediumseagreen" width="150"><font color="#ffffff">Prism diopter step</font></td>
                <td width="150">
                    <xsl:value-of select="$Conf/PrismDiopterStep" /><xsl:if test="$Conf/PrismDiopterStep/@unit[. = 'pri']"><xsl:text>△</xsl:text></xsl:if>
                </td>
            </tr>
            <tr align="center">
                <td bgcolor="mediumseagreen" width="150"><font color="#ffffff">Prism base step</font></td>
                <td width="150">
                    <xsl:value-of select="$Conf/PrismBaseStep" /><xsl:if test="$Conf/PrismBaseStep/@unit[. = 'deg']"><xsl:text>°</xsl:text></xsl:if>
                </td>
            </tr>
            <tr align="center">
                <td bgcolor="mediumseagreen" width="150"><font color="#ffffff">Prism mode</font></td>
                <td width="150"><xsl:value-of select="$Conf/PrismMode" /></td>
            </tr>
            <tr align="center">
                <td bgcolor="mediumseagreen" width="150"><font color="#ffffff">Add mode</font></td>
                <td width="150"><xsl:value-of select="$Conf/AddMode" /></td>
            </tr>

        </table>

    </xsl:template>

    <!--==========================    LM measurement items    ==========================-->
    <xsl:template name="LM-SRL">
        <xsl:param name="SRL-value" />

        <xsl:choose>

            <xsl:when test="$SRL-value/Error">

                <table border="1" Cellspacing="0">
                    <tr align="center" bgcolor="#FFA500">
                        <td width="320"><font color="#ffffff">Error</font></td>
                    </tr>

                    <xsl:call-template name="lmerror_data">
                        <xsl:with-param name="lmerror_value" select="$SRL-value" />
                    </xsl:call-template>

                </table>

            </xsl:when>

            <xsl:otherwise>

                <table border="1" Cellspacing="0">
                    <tr align="center" bgcolor="#FFA500">
                        <td width="80"><font color="#ffffff">Sphere</font></td>
                        <td width="80"><font color="#ffffff">Cylinder</font></td>
                        <td width="80"><font color="#ffffff">Axis</font></td>
                        <td width="80"><font color="#ffffff">SE</font></td>
                        <td width="80"><font color="#ffffff">ADD</font></td>
                        <td width="80"><font color="#ffffff">ADD2</font></td>
                        <td width="80"><font color="#ffffff">NearSphere</font></td>
                        <td width="80"><font color="#ffffff">NearSphere2</font></td>
                        <td width="80"><font color="#ffffff">Prism</font></td>
                        <td width="80"><font color="#ffffff">Prism base</font></td>
                        <td width="80"><font color="#ffffff">Horizontal prism</font></td>
                        <td width="80"><font color="#ffffff">Vertical prism</font></td>
                        <td width="80"><font color="#ffffff">UV Transmittance</font></td>
                        <xsl:if test="$SRL-value/ConfidenceIndex">
                            <td width="80"><font color="#ffffff">ConfidenceIndex</font></td>
                        </xsl:if>
                    </tr>

                    <xsl:call-template name="lm_data">
                        <xsl:with-param name="lm_value" select="$SRL-value" />
                    </xsl:call-template>

                </table>

            </xsl:otherwise>

        </xsl:choose>

    </xsl:template>

    <!--==========================    LM measurement error data    ====================-->
    <xsl:template name="lmerror_data">
        <xsl:param name="lmerror_value" />
        <tr align="center">
            <td><xsl:value-of select="$lmerror_value/Error" /></td>
        </tr>

    </xsl:template>

    <!--==========================    LM measurement data    ==========================-->
    <xsl:template name="lm_data">
        <xsl:param name="lm_value" />
        <tr align="center">
            <xsl:choose>
                <xsl:when test="$lm_value/Sphare">
                    <td><xsl:value-of select="$lm_value/Sphare" /><xsl:if test="$lm_value/Sphare[. != '']"><xsl:value-of select="$lm_value/Sphare/@unit" /></xsl:if></td>
                </xsl:when>
                <xsl:otherwise>
                    <td><xsl:value-of select="$lm_value/Sphere" /><xsl:if test="$lm_value/Sphere[. != '']"><xsl:value-of select="$lm_value/Sphere/@unit" /></xsl:if></td>
                </xsl:otherwise>
            </xsl:choose>
            <td><xsl:value-of select="$lm_value/Cylinder" /><xsl:if test="$lm_value/Cylinder[. != '']"><xsl:value-of select="$lm_value/Cylinder/@unit" /></xsl:if></td>
            <td>
                <xsl:value-of select="$lm_value/Axis" /><xsl:if test="$lm_value/Axis[. != '']"><xsl:if test="$lm_value/Axis/@unit[. = 'deg']"><xsl:text>°</xsl:text></xsl:if></xsl:if>
            </td>

            <td><xsl:value-of select="$lm_value/SE" /><xsl:if test="$lm_value/SE[. != '']"><xsl:value-of select="$lm_value/SE/@unit" /></xsl:if></td>
            <td><xsl:value-of select="$lm_value/ADD" /><xsl:if test="$lm_value/ADD[. != '']"><xsl:value-of select="$lm_value/ADD/@unit" /></xsl:if></td>
            <td><xsl:value-of select="$lm_value/ADD2" /><xsl:if test="$lm_value/ADD2[. != '']"><xsl:value-of select="$lm_value/ADD2/@unit" /></xsl:if></td>
            <xsl:choose>
                <xsl:when test="$lm_value/NearSphare">
                    <td><xsl:value-of select="$lm_value/NearSphare" /><xsl:if test="$lm_value/NearSphare[. != '']"><xsl:value-of select="$lm_value/NearSphare/@unit" /></xsl:if></td>
                </xsl:when>
                <xsl:otherwise>
                    <td><xsl:value-of select="$lm_value/NearSphere" /><xsl:if test="$lm_value/NearSphere[. != '']"><xsl:value-of select="$lm_value/NearSphere/@unit" /></xsl:if></td>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="$lm_value/NearSphare2">
                    <td><xsl:value-of select="$lm_value/NearSphare2" /><xsl:if test="$lm_value/NearSphare2[. != '']"><xsl:value-of select="$lm_value/NearSphare2/@unit" /></xsl:if></td>
                </xsl:when>
                <xsl:otherwise>
                    <td><xsl:value-of select="$lm_value/NearSphere2" /><xsl:if test="$lm_value/NearSphere2[. != '']"><xsl:value-of select="$lm_value/NearSphere2/@unit" /></xsl:if></td>
                </xsl:otherwise>
            </xsl:choose>
            <td>
                <xsl:value-of select="$lm_value/Prism" /><xsl:if test="$lm_value/Prism[. != '']"><xsl:if test="$lm_value/Prism/@unit[. = 'pri']"><xsl:text>△</xsl:text></xsl:if></xsl:if>
            </td>
            <td>
                <xsl:value-of select="$lm_value/PrismBase" /><xsl:if test="$lm_value/PrismBase[. != '']"><xsl:if test="$lm_value/PrismBase/@unit[. = 'deg']"><xsl:text>°</xsl:text></xsl:if></xsl:if>
            </td>
            <td>
                <xsl:value-of select="$lm_value/PrismX/@base" /><xsl:value-of select="$lm_value/PrismX" /><xsl:if test="$lm_value/PrismX[. != '']"><xsl:if test="$lm_value/PrismX/@unit[. = 'pri']"><xsl:text>△</xsl:text></xsl:if></xsl:if>
            </td>
            <td>
                <xsl:value-of select="$lm_value/PrismY/@base" /><xsl:value-of select="$lm_value/PrismY" /><xsl:if test="$lm_value/PrismY[. != '']"><xsl:if test="$lm_value/PrismY/@unit[. = 'pri']"><xsl:text>△</xsl:text></xsl:if></xsl:if>
            </td>
            <td><xsl:value-of select="$lm_value/UVTransmittance" /><xsl:if test="$lm_value/UVTransmittance[. != '']"><xsl:value-of select="$lm_value/UVTransmittance/@unit" /></xsl:if></td>
            <xsl:if test="$lm_value/ConfidenceIndex">
                <td><xsl:value-of select="$lm_value/ConfidenceIndex" /></td>
            </xsl:if>
        </tr>

    </xsl:template>

    <!--==========================    PD measurement    ==========================-->
    <xsl:template name="PD">
        <xsl:param name="PD-value" />

        <table border="1" Cellspacing="0">
            <xsl:if test="$PD-value/Distance[. != ''] or $PD-value/DistanceR[. != ''] or $PD-value/DistanceL[. != '']">
                <tr align="center" bgcolor="#FFA500">
                    <td width="80"><font color="#ffffff">Distance</font></td>
                    <td width="80"><font color="#ffffff">Distance right</font></td>
                    <td width="80"><font color="#ffffff">Distance left</font></td>
                </tr>

                <tr align="center">
                    <td><xsl:value-of select="$PD-value/Distance" /><xsl:if test="$PD-value/Distance[. != '']"><xsl:value-of select="$PD-value/Distance/@unit" /></xsl:if></td>
                    <td><xsl:value-of select="$PD-value/DistanceR" /><xsl:if test="$PD-value/DistanceR[. != '']"><xsl:value-of select="$PD-value/DistanceR/@unit" /></xsl:if></td>
                    <td><xsl:value-of select="$PD-value/DistanceL" /><xsl:if test="$PD-value/DistanceL[. != '']"><xsl:value-of select="$PD-value/DistanceL/@unit" /></xsl:if></td>
                </tr>
            </xsl:if>

            <xsl:if test="$PD-value/Near[. != ''] or $PD-value/NearR[. != ''] or $PD-value/NearL[. != '']">
                <tr align="center" bgcolor="#FFA500">
                    <td width="80"><font color="#ffffff">Near</font></td>
                    <td width="80"><font color="#ffffff">Near right</font></td>
                    <td width="80"><font color="#ffffff">Near left</font></td>
                </tr>

                <tr align="center">
                    <td><xsl:value-of select="$PD-value/Near" /><xsl:if test="$PD-value/Near[. != '']"><xsl:value-of select="$PD-value/Near/@unit" /></xsl:if></td>
                    <td><xsl:value-of select="$PD-value/NearR" /><xsl:if test="$PD-value/NearR[. != '']"><xsl:value-of select="$PD-value/NearR/@unit" /></xsl:if></td>
                    <td><xsl:value-of select="$PD-value/NearL" /><xsl:if test="$PD-value/NearL[. != '']"><xsl:value-of select="$PD-value/NearL/@unit" /></xsl:if></td>
                </tr>
            </xsl:if>

        </table>

    </xsl:template>

    <!--==========================    NIDEK LM measurement    ==========================-->
    <xsl:template name="NIDEK">
        <xsl:param name="NIDEK-value" />

        <xsl:if test="$NIDEK-value/S">
            <xsl:text>Single</xsl:text>

            <xsl:call-template name="NIDEK-SRL">
                <xsl:with-param name="SRL-value" select="$NIDEK-value/S" />
            </xsl:call-template>

            <br />
        </xsl:if>

        <xsl:if test="$NIDEK-value/R">
            <xsl:text>Right side</xsl:text>

            <xsl:call-template name="NIDEK-SRL">
                <xsl:with-param name="SRL-value" select="$NIDEK-value/R" />
            </xsl:call-template>

            <br />
        </xsl:if>

        <xsl:if test="$NIDEK-value/L">
            <xsl:text>Left side</xsl:text>

            <xsl:call-template name="NIDEK-SRL">
                <xsl:with-param name="SRL-value" select="$NIDEK-value/L" />
            </xsl:call-template>

            <br />
        </xsl:if>

        <xsl:if test="$NIDEK-value/NetPrism">
            <xsl:text>Net prism</xsl:text>

            <table border="1" Cellspacing="0">
                <tr align="center" bgcolor="#FFA500">
                    <td width="160"><font color="#ffffff">Net horizontal prism</font></td>
                    <td width="160"><font color="#ffffff">Net vertical prism</font></td>
                </tr>

                <tr align="center">
                    <td>
                        <xsl:value-of select="$NIDEK-value/NetPrism/NetHPrism/@base" /><xsl:value-of select="$NIDEK-value/NetPrism/NetHPrism" /><xsl:if test="$NIDEK-value/NetPrism/NetHPrism[. != '']"><xsl:if test="$NIDEK-value/NetPrism/NetHPrism/@unit[. = 'pri']"><xsl:text>△</xsl:text></xsl:if></xsl:if>
                    </td>
                    <td>
                        <xsl:value-of select="$NIDEK-value/NetPrism/NetVPrism/@base" /><xsl:value-of select="$NIDEK-value/NetPrism/NetVPrism" /><xsl:if test="$NIDEK-value/NetPrism/NetVPrism[. != '']"><xsl:if test="$NIDEK-value/NetPrism/NetVPrism/@unit[. = 'pri']"><xsl:text>△</xsl:text></xsl:if></xsl:if>
                    </td>
                </tr>

            </table>

            <br />
        </xsl:if>

        <xsl:if test="$NIDEK-value/Inside">
            <xsl:text>Inside</xsl:text>

            <table border="1" Cellspacing="0">
                <tr align="center" bgcolor="#FFA500">
                    <td width="160"><font color="#ffffff">Inside right</font></td>
                    <td width="160"><font color="#ffffff">Inside left</font></td>
                </tr>

                <tr align="center">
                    <td><xsl:value-of select="$NIDEK-value/Inside/InsideR" /><xsl:if test="$NIDEK-value/Inside/InsideR[. != '']"><xsl:value-of select="$NIDEK-value/Inside/InsideR/@unit" /></xsl:if></td>
                    <td><xsl:value-of select="$NIDEK-value/Inside/InsideL" /><xsl:if test="$NIDEK-value/Inside/InsideL[. != '']"><xsl:value-of select="$NIDEK-value/Inside/InsideL/@unit" /></xsl:if></td>
                </tr>

            </table>

            <br />
        </xsl:if>

        <xsl:if test="$NIDEK-value/MAP">
            <xsl:text>MAP</xsl:text>

            <br />

                <xsl:if test="$NIDEK-value/MAP/S">
                    <xsl:text>Single</xsl:text>

                    <xsl:call-template name="MAP-SRL">
                        <xsl:with-param name="SRL-value" select="$NIDEK-value/MAP/S" />
                    </xsl:call-template>

                    <br />
                </xsl:if>

                <xsl:if test="$NIDEK-value/MAP/R">
                    <xsl:text>Right side</xsl:text>

                    <xsl:call-template name="MAP-SRL">
                        <xsl:with-param name="SRL-value" select="$NIDEK-value/MAP/R" />
                    </xsl:call-template>

                    <br />
                </xsl:if>

                <xsl:if test="$NIDEK-value/MAP/L">
                    <xsl:text>Left side</xsl:text>

                    <xsl:call-template name="MAP-SRL">
                        <xsl:with-param name="SRL-value" select="$NIDEK-value/MAP/L" />
                    </xsl:call-template>

                    <br />
                </xsl:if>

            <br />
        </xsl:if>

    </xsl:template>

    <!--==========================    NIDEK measurement items    ==========================-->
    <xsl:template name="NIDEK-SRL">
        <xsl:param name="SRL-value" />

        <table border="1" Cellspacing="0">
            <tr align="center" bgcolor="#FFA500">
                <td width="80"><font color="#ffffff">Length</font></td>
                <td width="80"><font color="#ffffff">Channel width</font></td>
                <td width="80"><font color="#ffffff">Channel length</font></td>
                <td width="80"><font color="#ffffff">Index</font></td>
                <xsl:if test="$SRL-value/GreenTransmittance">
                    <td width="80"><font color="#ffffff">Green Transmittance</font></td>
                </xsl:if>
            </tr>

            <xsl:call-template name="nidek_data">
                <xsl:with-param name="nidek_value" select="$SRL-value" />
            </xsl:call-template>

        </table>

    </xsl:template>

    <!--==========================    NIDEK measurement data    ==========================-->
    <xsl:template name="nidek_data">
        <xsl:param name="nidek_value" />
        <tr align="center">
            <td><xsl:value-of select="$nidek_value/Length" /><xsl:if test="$nidek_value/Length[. != '']"><xsl:value-of select="$nidek_value/Length/@unit" /></xsl:if></td>
            <td><xsl:value-of select="$nidek_value/ChannelWidth" /><xsl:if test="$nidek_value/ChannelWidth[. != '']"><xsl:value-of select="$nidek_value/ChannelWidth/@unit" /></xsl:if></td>
            <td><xsl:value-of select="$nidek_value/ChannelLength" /><xsl:if test="$nidek_value/ChannelLength[. != '']"><xsl:value-of select="$nidek_value/ChannelLength/@unit" /></xsl:if></td>
            <td><xsl:value-of select="$nidek_value/Index" /></td>
            <xsl:if test="$nidek_value/GreenTransmittance">
                <td><xsl:value-of select="$nidek_value/GreenTransmittance" /><xsl:if test="$nidek_value/GreenTransmittance[. != '']"><xsl:value-of select="$nidek_value/GreenTransmittance/@unit" /></xsl:if></td>
            </xsl:if>
            </tr>

    </xsl:template>

    <!--==========================    MAP measurement    ==========================-->
    <xsl:template name="MAP-SRL">
        <xsl:param name="SRL-value" />

        <br />
        <xsl:text>Measurement conditions</xsl:text>

        <xsl:call-template name="MAP-Conf">
            <xsl:with-param name="Conf" select="$SRL-value" />
        </xsl:call-template>

        <br />
        <xsl:text>Sphere matrix</xsl:text>
        <xsl:choose>
            <xsl:when test="$SRL-value/SphareMatrix">
                <xsl:call-template name="map_data">
                    <xsl:with-param name="map_value" select="$SRL-value/SphareMatrix" />
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <xsl:call-template name="map_data">
                    <xsl:with-param name="map_value" select="$SRL-value/SphereMatrix" />
                </xsl:call-template>
            </xsl:otherwise>
        </xsl:choose>

        <br />
        <xsl:text>Cylinder matrix</xsl:text>
        <xsl:call-template name="map_data">
            <xsl:with-param name="map_value" select="$SRL-value/CylinderMatrix" />
        </xsl:call-template>

    </xsl:template>

    <!--==========================    MAP measurement conditions    ==========================-->
    <xsl:template name="MAP-Conf">
        <xsl:param name="Conf" />
        <table border="1" Cellspacing="0">
            <tr align="center" bgcolor="#FFA500">
                <td width="80"><font color="#ffffff">Map mode</font></td>
                <td width="80"><font color="#ffffff">Area size</font></td>
                <td width="80"><font color="#ffffff">Matrix size X</font></td>
                <td width="80"><font color="#ffffff">Matrix size Y</font></td>
                <td width="80"><font color="#ffffff">Center sphere</font></td>
                <td width="80"><font color="#ffffff">Center cylinder</font></td>
                <td width="80"><font color="#ffffff">Upper limit sphere</font></td>
                <td width="80"><font color="#ffffff">Lower limit sphere</font></td>
                <td width="80"><font color="#ffffff">Upper limit cylinder</font></td>
                <td width="80"><font color="#ffffff">Lower limit cylinder</font></td>
            </tr>

            <tr align="center">
                <td><xsl:value-of select="$Conf/MapMode" /></td>
                <td><xsl:value-of select="$Conf/AreaSize" /></td>
                <td><xsl:value-of select="$Conf/MatrixSizeX" /></td>
                <td><xsl:value-of select="$Conf/MatrixSizeY" /></td>
                <xsl:choose>
                    <xsl:when test="$Conf/CenterSphare">
                        <td><xsl:value-of select="$Conf/CenterSphare" /><xsl:if test="$Conf/CenterSphare[. != '']"><xsl:value-of select="$Conf/CenterSphare/@unit" /></xsl:if></td>
                    </xsl:when>
                    <xsl:otherwise>
                        <td><xsl:value-of select="$Conf/CenterSphere" /><xsl:if test="$Conf/CenterSphere[. != '']"><xsl:value-of select="$Conf/CenterSphere/@unit" /></xsl:if></td>
                    </xsl:otherwise>
                </xsl:choose>
                <td><xsl:value-of select="$Conf/CenterCylinder" /><xsl:if test="$Conf/CenterCylinder[. != '']"><xsl:value-of select="$Conf/CenterCylinder/@unit" /></xsl:if></td>
                <td><xsl:value-of select="$Conf/UpperLimitSph" /><xsl:if test="$Conf/UpperLimitSph[. != '']"><xsl:value-of select="$Conf/UpperLimitSph/@unit" /></xsl:if></td>
                <td><xsl:value-of select="$Conf/LowerLimitSph" /><xsl:if test="$Conf/LowerLimitSph[. != '']"><xsl:value-of select="$Conf/LowerLimitSph/@unit" /></xsl:if></td>
                <td><xsl:value-of select="$Conf/UpperLimitCyl" /><xsl:if test="$Conf/UpperLimitCyl[. != '']"><xsl:value-of select="$Conf/UpperLimitCyl/@unit" /></xsl:if></td>
                <td><xsl:value-of select="$Conf/LowerLimitCyl" /><xsl:if test="$Conf/LowerLimitCyl[. != '']"><xsl:value-of select="$Conf/LowerLimitCyl/@unit" /></xsl:if></td>
            </tr>

        </table>

    </xsl:template>

    <!--==========================    MAP measurement items    ==========================-->
    <xsl:template name="map_data">
        <xsl:param name="map_value" />

        <table border="1" Cellspacing="0">
            <tr align="center" bgcolor="#FFA500">
                <td width="80"><font color="#ffffff"></font></td>
                <td width="80"><font color="#ffffff">1</font></td>
                <td width="80"><font color="#ffffff">2</font></td>
                <td width="80"><font color="#ffffff">3</font></td>
                <td width="80"><font color="#ffffff">4</font></td>
                <td width="80"><font color="#ffffff">5</font></td>
                <td width="80"><font color="#ffffff">6</font></td>
                <td width="80"><font color="#ffffff">7</font></td>
                <td width="80"><font color="#ffffff">8</font></td>
                <td width="80"><font color="#ffffff">9</font></td>
                <td width="80"><font color="#ffffff">10</font></td>
                <td width="80"><font color="#ffffff">11</font></td>
                <td width="80"><font color="#ffffff">12</font></td>
                <td width="80"><font color="#ffffff">13</font></td>
                <td width="80"><font color="#ffffff">14</font></td>
                <td width="80"><font color="#ffffff">15</font></td>
            </tr>

            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="1" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="2" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="3" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="4" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="5" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="6" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="7" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="8" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="9" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="10" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="11" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="12" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="13" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="14" />
            </xsl:call-template>
            <xsl:call-template name="mapsub_data">
                <xsl:with-param name="mapsub_value" select="$map_value" />
                <xsl:with-param name="mapcol_value" select="15" />
            </xsl:call-template>

        </table>

    </xsl:template>

    <!--==========================    MAP1-line measurement data    ==========================-->
    <xsl:template name="mapsub_data">
        <xsl:param name="mapsub_value" />
        <xsl:param name="mapcol_value" />

        <tr align="center">
            <td width="80" bgcolor="#FFA500"><font color="#ffffff"><xsl:value-of select="$mapcol_value" /></font></td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-14]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-13]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-12]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-11]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-10]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-9]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-8]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-7]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-6]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-5]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-4]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-3]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-2]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15-1]" /> </td>
            <td Width="80"><xsl:value-of select="$mapsub_value/Matrix[$mapcol_value*15]" /> </td>
        </tr>

    </xsl:template>

    <!--==========================    Common Data      ==========================-->
    <xsl:template name="Common">
        <xsl:param name="comm-value" />
        <table>
            <tr valign="top">
                <td>
                    <table border="1" Cellspacing="0">
                        <tr align="center">
                            <td bgcolor="#00CED1" width="150"><font color="#ffffff">Company</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Company" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#00CED1" width="150"><font color="#ffffff">Model name</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/ModelName" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#00CED1" width="150"><font color="#ffffff">Machine No</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/MachineNo" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#00CED1" width="150"><font color="#ffffff">ROM Version</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/ROMVersion" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#00CED1" width="150"><font color="#ffffff">XML Version</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Version" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#00CED1" width="150"><font color="#ffffff">Date</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Date" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#00CED1" width="150"><font color="#ffffff">Time</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Time" /></td>
                        </tr>

                    </table>

                </td>

                <td>
                    <table border="1" Cellspacing="0">
                        <tr align="center">
                            <td bgcolor="#008B8B" width="150"><font color="#ffffff">Patient No</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Patient/No." /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#008B8B" width="150"><font color="#ffffff">Patient ID</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Patient/ID" />
                            </td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#008B8B" width="150"><font color="#ffffff">Patient first name</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Patient/FirstName" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#008B8B" width="150"><font color="#ffffff">Patient middle name</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Patient/MiddleName" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#008B8B" width="150"><font color="#ffffff">Patient last name</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Patient/LastName" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#008B8B" width="150"><font color="#ffffff">Patient sex</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Patient/Sex" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#008B8B" width="150"><font color="#ffffff">Patient age</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Patient/Age" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#008B8B" width="150"><font color="#ffffff">Patient DOB</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Patient/DOB" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#008B8B" width="150"><font color="#ffffff">Patient NameJ1</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Patient/NameJ1" /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#008B8B" width="150"><font color="#ffffff">Patient NameJ2</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Patient/NameJ2" /></td>
                        </tr>

                    </table>

                </td>

                <td>
                    <table border="1" Cellspacing="0">
                        <tr align="center">
                            <td bgcolor="#5F9EA0" width="150"><font color="#ffffff">Operator No</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Operator/No." /></td>
                        </tr>
                        <tr align="center">
                            <td bgcolor="#5F9EA0" width="150"><font color="#ffffff">Operator ID</font></td>
                            <td width="100"><xsl:value-of select="$comm-value/Operator/ID" /></td>
                        </tr>

                    </table>

                </td>

            </tr>

        </table>

    </xsl:template>

    <!--============================================================================================-->
</xsl:stylesheet>