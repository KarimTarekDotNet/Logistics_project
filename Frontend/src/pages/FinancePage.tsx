  import { Banknote, CreditCard, FileText, Landmark, Plus, ReceiptText, Trash2, WalletCards } from "lucide-react";
import { useState, type FormEvent } from "react";
import { ConfirmDialog, EmptyState, Field, PanelTitle, SectionHeader, StatusBadge } from "../components/ui";
import { ShipmentContextPanel } from "../features/shipments/ShipmentContextPanel";
import type { Invoice, Shipment, ShipmentCharge } from "../types";
import { formatDate, formatMoney } from "../utils/format";

export function FinancePage(props: {
  selectedShipment?: Shipment;
  charges: ShipmentCharge[];
  invoices: Invoice[];
  isPrivileged: boolean;
  isAdmin: boolean;
  busy: boolean;
  onCreateInvoice: (event: FormEvent) => void;
  onLoadInvoices: () => void;
  onInvoiceStatus: (id: string, action: "mark-as-paid" | "mark-as-partially-paid" | "mark-as-refunded", price?: number) => void;
  onCancelInvoice: (id: string, reason: string) => void;
  onDeleteInvoice: (id: string) => void;
}) {
  const {
    selectedShipment,
    charges,
    invoices,
    isPrivileged,
    isAdmin,
    busy,
    onCreateInvoice,
    onLoadInvoices,
    onInvoiceStatus,
    onCancelInvoice,
    onDeleteInvoice
  } = props;
  const [partialAmounts, setPartialAmounts] = useState<Record<string, string>>({});
  const [deleteInvoiceId, setDeleteInvoiceId] = useState<string | null>(null);
  const [cancelInvoiceId, setCancelInvoiceId] = useState<string | null>(null);
  const [cancelReason, setCancelReason] = useState("Cancelled from operations console");

  const chargeTotal = charges.reduce((total, charge) => total + charge.amount + charge.taxAmount, 0);

  function resolveInvoiceBalance(invoice: Invoice) {
    const explicitRemaining =
      invoice.remainingAmount ?? invoice.remainingBalance ?? invoice.amountDue ?? invoice.balanceDue;
    const paidPart = Math.max(0, Number(invoice.paidPart ?? 0));
    const total = Math.max(0, Number(invoice.totalAmount ?? 0));
    const status = invoice.paymentStatus.toLowerCase();
    const isPaid = status === "paid" || (status.includes("paid") && !status.includes("partial"));
    const remaining =
      explicitRemaining != null
        ? Math.max(0, Number(explicitRemaining))
        : isPaid
          ? 0
          : Math.max(0, total - paidPart);

    return { total, paidPart, remaining, hasPartialPayment: paidPart > 0 && remaining > 0 && remaining < total };
  }

  function normalizePaymentStatus(status: string) {
    return status.replace(/\s+/g, "").toLowerCase();
  }

  const invoiceCurrency = invoices[0]?.currency ?? selectedShipment?.currency ?? "USD";
  const invoiceSummary = invoices.reduce(
    (summary, invoice) => {
      const balance = resolveInvoiceBalance(invoice);
      const status = normalizePaymentStatus(invoice.paymentStatus);

      return {
        total: summary.total + balance.total,
        paid: summary.paid + (status === "paid" ? balance.total : balance.paidPart),
        remaining: summary.remaining + balance.remaining,
        actionable: summary.actionable + (status === "pending" || status === "partiallypaid" ? 1 : 0)
      };
    },
    { total: 0, paid: 0, remaining: 0, actionable: 0 }
  );

  return (
    <div className="view-stack">
      <SectionHeader icon={<WalletCards size={22} />} title="Finance" meta={selectedShipment ? `Shipment finance` : "No shipment"} />

      {selectedShipment ? (
        <>
          <ShipmentContextPanel
            shipment={selectedShipment}
            extra={[
              { label: "Invoices", value: String(invoices.length) },
              { label: "Outstanding", value: formatMoney(invoiceSummary.remaining, invoiceCurrency) }
            ]}
          />

          <section className="panel finance-invoices-panel">
              <div className="panel-title-row">
                <PanelTitle icon={<FileText size={18} />} title="Invoices" />
                <button className="mini-button" type="button" onClick={onLoadInvoices} disabled={busy}>
                  Load
                </button>
              </div>
              <div className="payment-overview-grid">
                <div>
                  <span>Total invoices</span>
                  <strong>{formatMoney(invoiceSummary.total, invoiceCurrency)}</strong>
                </div>
                <div>
                  <span>Collected</span>
                  <strong>{formatMoney(invoiceSummary.paid, invoiceCurrency)}</strong>
                </div>
                <div>
                  <span>Outstanding</span>
                  <strong>{formatMoney(invoiceSummary.remaining, invoiceCurrency)}</strong>
                </div>
                <div>
                  <span>Payable</span>
                  <strong>{invoiceSummary.actionable}</strong>
                </div>
              </div>
              {isPrivileged && (
                <form className="form-stack" onSubmit={onCreateInvoice}>
                  <div className="invoice-draft-summary">
                    <span>{charges.length > 0 ? "Ready to draft" : "No billing lines"}</span>
                    <strong>{formatMoney(chargeTotal, selectedShipment.currency)}</strong>
                  </div>
                  <button className="primary-button compact" type="submit" disabled={busy || charges.length === 0}>
                    <Plus size={17} />
                    Create draft invoice
                  </button>
                </form>
              )}

              <div className="compact-list invoice-list">
                {invoices.map((invoice) => {
                  const balance = resolveInvoiceBalance(invoice);
                  const status = normalizePaymentStatus(invoice.paymentStatus);
                  const isDraft = status === "draft";
                  const isPending = status === "pending";
                  const isCancelled = status === "cancelled";
                  const isRefunded = status === "refunded";
                  const isPaid = status === "paid";
                  const isPartiallyPaid = status === "partiallypaid";
                  const canPay = (isPending || isPartiallyPaid) && !isPaid && !isCancelled && !isRefunded;
                  const canPartialPay = isPrivileged && canPay && balance.remaining > 0;
                  const canRefund = isPrivileged && isPending && !isCancelled && !isRefunded;
                  const canCancel = (isDraft || isPending) && !isPaid && !isPartiallyPaid && !isCancelled && !isRefunded;
                  const paymentLabel = isPaid
                    ? "Settled"
                    : isPartiallyPaid
                      ? "Part paid"
                      : isDraft
                        ? "Draft"
                        : isCancelled
                          ? "Cancelled"
                          : isRefunded
                            ? "Refunded"
                            : "Payment due";

                  return (
                    <div className="invoice-row payment-card" key={invoice.id}>
                      <div className="invoice-card-head">
                        <div className="invoice-title-block">
                          <span className="invoice-kicker">
                            <ReceiptText size={14} />
                            Invoice
                          </span>
                          <strong>{invoice.invoiceNumber}</strong>
                          <small>
                            Issued {formatDate(invoice.issuedAt)} - Due {formatDate(invoice.dueDate)}
                          </small>
                        </div>
                        <div className="invoice-status-stack">
                          <StatusBadge status={invoice.paymentStatus} />
                          <span className="payment-state">{paymentLabel}</span>
                        </div>
                      </div>

                      <div className="payment-summary-grid">
                        <div>
                          <span>Total</span>
                          <strong>{formatMoney(balance.total, invoice.currency)}</strong>
                        </div>
                        <div>
                          <span>Paid</span>
                          <strong>{formatMoney(isPaid ? balance.total : balance.paidPart, invoice.currency)}</strong>
                        </div>
                        <div>
                          <span>Remaining</span>
                          <strong>{formatMoney(balance.remaining, invoice.currency)}</strong>
                        </div>
                        <div>
                          <span>Payer</span>
                          <strong>{invoice.payerType || "Customer"}</strong>
                        </div>
                      </div>

                      <div className="payment-method-strip">
                        <span>
                          <CreditCard size={14} />
                          Gateway
                        </span>
                        <span>
                          <Banknote size={14} />
                          Cash
                        </span>
                        <span>
                          <Landmark size={14} />
                          Bank transfer
                        </span>
                      </div>

                      <div className="invoice-actions payment-actions">
                        <div className="invoice-balance">
                          <span>{formatMoney(balance.remaining, invoice.currency)}</span>
                          {balance.hasPartialPayment && (
                            <small>
                              Remaining after {formatMoney(balance.paidPart, invoice.currency)} paid
                            </small>
                          )}
                        </div>
                        <button className="mini-button" type="button" onClick={() => onInvoiceStatus(invoice.id, "mark-as-paid")} disabled={busy || !canPay}>
                          Paid
                        </button>
                        {canPartialPay && (
                          <>
                            <div className="partial-pay-row">
                              <input
                                type="number"
                                min="0.01"
                                max={balance.remaining || undefined}
                                step="0.01"
                                placeholder="Amount"
                                className="mini-input"
                                value={partialAmounts[invoice.id] ?? ""}
                                onChange={(event) => setPartialAmounts((current) => ({ ...current, [invoice.id]: event.target.value }))}
                              />
                              <button
                                className="mini-button"
                                type="button"
                                disabled={
                                  !partialAmounts[invoice.id] ||
                                  Number(partialAmounts[invoice.id]) <= 0 ||
                                  Number(partialAmounts[invoice.id]) > balance.remaining
                                }
                                onClick={() => {
                                  const price = Number(partialAmounts[invoice.id]);
                                  if (!price || price <= 0 || price > balance.remaining) return;
                                  onInvoiceStatus(invoice.id, "mark-as-partially-paid", price);
                                  setPartialAmounts((current) => ({ ...current, [invoice.id]: "" }));
                                }}
                              >
                                Partial
                              </button>
                            </div>
                          </>
                        )}
                        {isPrivileged && (
                          <button className="mini-button" type="button" onClick={() => onInvoiceStatus(invoice.id, "mark-as-refunded")} disabled={busy || !canRefund}>
                            Refund
                          </button>
                        )}
                        <button className="mini-button danger" type="button" onClick={() => setCancelInvoiceId(invoice.id)} disabled={busy || !canCancel}>
                          Cancel
                        </button>
                        {isAdmin && (
                          <button className="icon-mini danger" type="button" onClick={() => setDeleteInvoiceId(invoice.id)} title="Delete invoice">
                            <Trash2 size={14} />
                          </button>
                        )}
                      </div>
                    </div>
                  );
                })}
                {invoices.length === 0 && <EmptyState icon={<FileText size={24} />} title="No invoices loaded" description="Load shipment invoices or create one when billing lines are ready." />}
              </div>
          </section>
        </>
      ) : (
        <EmptyState icon={<WalletCards size={28} />} title="No shipment selected" />
      )}

      <ConfirmDialog
        open={Boolean(deleteInvoiceId)}
        title="Delete invoice"
        message="This admin-only action removes the invoice record."
        confirmLabel="Delete invoice"
        tone="danger"
        busy={busy}
        onClose={() => setDeleteInvoiceId(null)}
        onConfirm={() => {
          if (!deleteInvoiceId) return;
          onDeleteInvoice(deleteInvoiceId);
          setDeleteInvoiceId(null);
        }}
      />

      <ConfirmDialog
        open={Boolean(cancelInvoiceId)}
        title="Cancel invoice"
        message={cancelReason}
        confirmLabel="Cancel invoice"
        tone="danger"
        busy={busy}
        onClose={() => setCancelInvoiceId(null)}
        onConfirm={() => {
          if (!cancelInvoiceId) return;
          onCancelInvoice(cancelInvoiceId, cancelReason);
          setCancelInvoiceId(null);
        }}
      />
      {cancelInvoiceId && (
        <div className="floating-reason">
          <Field label="Cancellation reason">
            <input value={cancelReason} onChange={(event) => setCancelReason(event.target.value.slice(0, 300))} maxLength={300} />
          </Field>
        </div>
      )}
    </div>
  );
}
