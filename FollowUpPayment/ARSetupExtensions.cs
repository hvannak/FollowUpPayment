using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects;
using System.Collections.Generic;
using System.Globalization;
using System;

namespace PX.Objects.AR
{
  public class ARSetupExt : PXCacheExtension<PX.Objects.AR.ARSetup>
  {
    #region UsrTelegramCC
    [PXDBString(50)]
    [PXUIField(DisplayName="Telegram CC")]

    public virtual string UsrTelegramCC { get; set; }
    public abstract class usrTelegramCC : PX.Data.BQL.BqlString.Field<usrTelegramCC> { }
    #endregion
  }
}