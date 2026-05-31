import { Calculator, CircleDollarSign, PackageCheck, Pencil } from "lucide-react";
import { EmptyState, PanelTitle, SectionHeader } from "../components/ui";
import { ShipmentContextPanel } from "../features/shipments/ShipmentContextPanel";
import type { Shipment, ShipmentCharge } from "../types";
import { formatMoney } from "../utils/format";

export function ChargeGenerationPage(props: {
  selectedShipment?: Shipment;
  charges: ShipmentCharge[];
  busy: boolean;
  onGenerate: () => void;
  onUpdateItems: () => void;
}) {
  const { selectedShipment, charges, busy, onGenerate, onUpdateItems } = props;
  const chargeTotal = charges.reduce((total, charge) => total + charge.amount + charge.taxAmount, 0);

  return (
    <div className="view-stack workflow-page">
      <SectionHeader icon={<Calculator size={22} />} title="Charge generation" meta={selectedShipment ? "Shipment charge cycle" : "No shipment"} />

      {selectedShipment ? (
        <>
          <section className="workspace-hero workflow-hero">
            <div className="workspace-hero-copy">
              <span className="hero-kicker">Billing step 1</span>
              <h2>Generate charge lines from the confirmed cargo profile before drafting the invoice.</h2>
              <p>Use this focused step when cargo is ready and finance needs clean charge visibility before invoice review.</p>
            </div>
            <div className="hero-metric-strip">
              <div>
                <span>Charges</span>
                <strong>{charges.length}</strong>
              </div>
              <div>
                <span>Total</span>
                <strong>{formatMoney(chargeTotal, selectedShipment.currency)}</strong>
              </div>
              <div>
                <span>Status</span>
                <strong>{selectedShipment.status}</strong>
              </div>
            </div>
          </section>

          <ShipmentContextPanel
            shipment={selectedShipment}
            extra={[
              { label: "Current charges", value: String(charges.length) },
              { label: "Charge total", value: formatMoney(chargeTotal, selectedShipment.currency) }
            ]}
          />

          <section className="workflow-center-panel">
            <div className="workflow-center-copy">
              <span className="workflow-step-mark">2</span>
              <h2>Generate shipment charges</h2>
              <p>Charges are calculated from the active rules for this shipment currency, cargo totals, volume, and agreed value.</p>
            </div>
            <button className="primary-button workflow-primary-action" type="button" onClick={onGenerate} disabled={busy}>
              <Calculator size={22} />
              Generate
            </button>
            <button className="secondary-button compact" type="button" onClick={onUpdateItems} disabled={busy}>
              <Pencil size={16} />
              Update items
            </button>
          </section>

          <section className="panel">
            <PanelTitle icon={<CircleDollarSign size={18} />} title="Charges preview" />
            <div className="compact-list">
              {charges.map((charge) => (
                <div className="list-row" key={charge.id}>
                  <div>
                    <strong>{charge.description}</strong>
                    <small>
                      {charge.chargeType} - {charge.payerType}
                    </small>
                  </div>
                  <span>{formatMoney(charge.amount + charge.taxAmount, charge.currency)}</span>
                </div>
              ))}
              {charges.length === 0 && <EmptyState icon={<PackageCheck size={24} />} title="No charges generated yet" />}
            </div>
          </section>
        </>
      ) : (
        <EmptyState icon={<Calculator size={28} />} title="No shipment selected" />
      )}
    </div>
  );
}
