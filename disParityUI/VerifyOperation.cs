﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using disParity;

namespace disParityUI
{

  internal class VerifyOperation : CancellableOperation
  {

    protected override void DoOperation()
    {

      if (anyDriveNeedsUpdate)
        foreach (DataDrive d in viewModel.ParitySet.Drives)
          if (d.Deletes.Count > 0) {
            MessageWindow.Show(viewModel.Owner, "Update before verify", "One or more drives have changes that must be processed before a Verify can be run.  Please Update first.", MessageWindowIcon.Caution, MessageWindowButton.OK);
            return;
          }

      viewModel.ParitySet.Verify();

      if (cancelled)
        Status = "Verify cancelled." + ((errorMessages.Count == 0) ? "" : (" Errors found: " + errorMessages.Count));
      else
        Status = "Verify complete.  " + ((errorMessages.Count == 0) ? "No errors found." : (" Errors found: " + viewModel.ParitySet.VerifyErrors));

    }

    protected override void CancelOperation()
    {
      viewModel.ParitySet.CancelVerify();
    }

    protected override string Name { get { return "Verify"; } }

    protected override string LowerCaseName { get { return "verify"; } }

  }

}
